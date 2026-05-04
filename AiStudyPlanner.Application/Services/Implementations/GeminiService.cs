using AiStudyPlanner.Application.Services.Interfaces;
using AiStudyPlanner.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AiStudyPlanner.Application.Services.Implementations
{
    public class GeminiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public GeminiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("API Key missing");
            _baseUrl = config["Gemini:BaseUrl"] ?? throw new ArgumentNullException("Base URL missing");
        }

        public async Task<AiResponse> GetStudyPlanAsync(string userInput)
        {
            var url = $"{_baseUrl}?key={_apiKey}";

            var prompt = $@"
                You are a professional study planner.

                User request:
                {userInput}

                Respond ONLY with valid JSON in this exact format:

                {{
                  ""tasks"": [""task1"", ""task2"", ""task3""],
                  ""estimatedTime"": ""string"",
                  ""priority"": ""Low | Medium | High""
                }}

                No explanations. No markdown. No extra text.
                ";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            // 🔁 RETRY LOGIC (3 attempts)
            HttpResponseMessage? response = null;

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                    break;

                if (response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    if (attempt == 3)
                        throw new Exception("AI service is busy. Please try again later.");

                    await Task.Delay(1000 * attempt); // 1s → 2s → 3s
                    continue;
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"AI service error {(int)response.StatusCode}: {errorBody}");
            }

            if (response == null)
                throw new Exception("No response from AI service.");

            var jsonString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(jsonString);

            if (!doc.RootElement.TryGetProperty("candidates", out var candidates) ||
                candidates.GetArrayLength() == 0)
            {
                throw new Exception("AI returned empty response.");
            }

            var rawAiOutput = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(rawAiOutput))
                throw new Exception("AI returned empty text.");

            // 🧹 CLEAN JSON
            string cleanedJson = rawAiOutput.Trim();

            if (cleanedJson.StartsWith("```"))
            {
                int firstLineEnd = cleanedJson.IndexOf('\n');
                if (firstLineEnd > 0)
                {
                    cleanedJson = cleanedJson.Substring(firstLineEnd).Replace("```", "").Trim();
                }
            }

            // 🔐 SAFE DESERIALIZATION
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<AiResponse>(cleanedJson, options);

            if (result == null || result.Tasks == null || !result.Tasks.Any())
                throw new Exception("Invalid AI response format.");

            return result;
        }
    }
}