using AiStudyPlanner.Application.Exceptions;
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
        private readonly bool _useMockAi;

        public GeminiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _useMockAi = config.GetValue<bool>("AiSettings:UseMockAi");

            if (!_useMockAi) 
            {
                _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("API Key missing");
                _baseUrl = config["Gemini:BaseUrl"] ?? throw new ArgumentNullException("Base URL missing");
            }
            else
            {
                _apiKey = string.Empty;
                _baseUrl = string.Empty;
            }

            Console.WriteLine($"DEBUG - UseMockAi: {_useMockAi}");
        }

        public async Task<AiResponse> GetStudyPlanAsync(string userInput)
        {
            if (_useMockAi)
            {
                return new AiResponse
                {
                    Tasks = new List<TaskItem>
                    {
                        new TaskItem
                        {
                            Id = Guid.NewGuid(),
                            Title = "Review core backend concepts",
                            IsCompleted = false
                        },
                        new TaskItem
                        {
                            Id = Guid.NewGuid(),
                            Title = "Practice common interview questions",
                            IsCompleted = false
                        },
                        new TaskItem
                        {
                            Id = Guid.NewGuid(),
                            Title = "Build one small coding exercise",
                            IsCompleted = false
                        }
                    },
                    EstimatedTime = "3 days",
                    Priority = "High"
                };
            }

            var url = $"{_baseUrl}?key={_apiKey}";

            var prompt = $@"
                You are a professional study and interview preparation planner.
                Your ONLY job is to help users with:
                - Study plans
                - Interview preparation
                - Learning roadmaps
                - Skill development for technical or academic topics

                User request:
                {userInput}

                IMPORTANT: If the user request is NOT related to studying, learning, or interview preparation,
                respond ONLY with this exact JSON and nothing else:

                {{
                  ""error"": ""off_topic""
                }}

                Otherwise, respond ONLY with valid JSON in this exact format:

                {{
                  ""tasks"": [
                    {{ ""title"": ""Task 1"", ""isCompleted"": false }},
                    {{ ""title"": ""Task 2"", ""isCompleted"": false }},
                    {{ ""title"": ""Task 3"", ""isCompleted"": false }}
                  ],
                  ""estimatedTime"": ""string"",
                  ""priority"": ""Low | Medium | High""
                }}

                Rules:
                - No explanations
                - No markdown
                - No extra text
                - Every task must have title and isCompleted
                - isCompleted must always be false for new generated tasks
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

            using var checkDoc = JsonDocument.Parse(cleanedJson);
            if (checkDoc.RootElement.TryGetProperty("error", out var errorProp) &&
                errorProp.GetString() == "off_topic")
            {
                throw new OffTopicRequestException();
            }

            // 🔐 SAFE DESERIALIZATION
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<AiResponse>(cleanedJson, options);

            if (result == null || result.Tasks == null || !result.Tasks.Any())
                throw new Exception("Invalid AI response format.");

            if (result.Tasks.Any(t => string.IsNullOrWhiteSpace(t.Title)))
                throw new Exception("AI returned one or more empty tasks.");

            foreach (var task in result.Tasks)
            {
                if (task.Id == Guid.Empty)
                    task.Id = Guid.NewGuid();

                task.IsCompleted = false;
            }

            return result;
        }
    }
}