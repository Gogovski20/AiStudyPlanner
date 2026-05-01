using AiStudyPlanner.Application.Services.Interfaces;
using AiStudyPlanner.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;

namespace AiStudyPlanner.Application.Services.Implementations
{
    public class GeminiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = string.Empty;
        private readonly string _baseUrl = string.Empty;

        public GeminiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("API Key missing");
            Console.WriteLine($"DEBUG - API Key starts with: {_apiKey?[..6]}...");
            _baseUrl = config["Gemini:BaseUrl"] ?? throw new ArgumentNullException("Base URL missing");
        }

        public async Task<AiResponse?> GetStudyPlanAsync(string userInput)
        {
            // 1. Build the URL
            var url = $"{_baseUrl}?key={_apiKey}";

            // 2. Prepare the Prompt (Strict instructions for the AI)
            var prompt = $@"
            You are a professional study planner.

            User request:
            {userInput}

            Respond ONLY with valid JSON in this exact format:

            {{
              ""tasks"": [""task1"", ""task2""],
              ""estimatedTime"": ""string"",
              ""priority"": ""Low | Medium | High""
            }}

            No explanations. No markdown. No extra text.
            ";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };

            try
            {
                // 3. Make the Request
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                Console.WriteLine($"FULL REQUEST URL: {url}");
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        throw new HttpRequestException("Rate limit hit. Wait 60 seconds and try again.");

                    throw new HttpRequestException($"Gemini API error {(int)response.StatusCode}: {errorBody}");
                }

                // 4. Extract the nested text from Google's response
                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);

                if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                {
                    Console.WriteLine("No candidates returned from AI.");
                    return null;
                }

                var rawAiOutput = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(rawAiOutput)) return null;

                // 5. THE CLEANER: Remove Markdown backticks if Gemini ignores instructions
                string cleanedJson = rawAiOutput.Trim();
                if (cleanedJson.StartsWith("```"))
                {
                    // Remove the opening ```json or ```
                    int firstLineEnd = cleanedJson.IndexOf('\n');
                    cleanedJson = cleanedJson.Substring(firstLineEnd).Replace("```", "").Trim();
                }

                // 6. Deserialize into your Domain Model
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<AiResponse>(cleanedJson, options);
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log the actual exception to help us debug
                Console.WriteLine($"Critical Error in AI Service: {ex.Message}");
                throw;
            }
        }
    }
}
