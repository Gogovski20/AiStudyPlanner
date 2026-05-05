using AiStudyPlanner.API.Contracts.Ai;
using AiStudyPlanner.Domain.Models;
using System.Security.Cryptography.X509Certificates;

namespace AiStudyPlanner.API.Mappers
{
    public static class AiResponseMapper
    {
        public static GenerateStudyPlanResponse ToGenerateStudyPlanResponse(ChatHistory chat)
        {
            return new GenerateStudyPlanResponse
            {
                Id = chat.Id,
                Tasks = ToTaskItemResponses(chat.Tasks),
                EstimatedTime = chat.EstimatedTime,
                Priority = chat.Priority,
                Progress = CalculateProgress(chat.Tasks)
            };
        }

        public static ChatHistoryResponse ToChatHistoryResponse(ChatHistory chat)
        {
            return new ChatHistoryResponse
            {
                Id = chat.Id,
                UserInput = chat.UserInput,
                Tasks = ToTaskItemResponses(chat.Tasks),
                EstimatedTime = chat.EstimatedTime,
                Priority = chat.Priority,
                CreatedAt = chat.CreatedAt,
                Progress = CalculateProgress(chat.Tasks)
            };
        }

        public static TaskItemResponse ToTaskItemResponse(TaskItem task)
        {
            return new TaskItemResponse
            {
                Title = task.Title,
                IsCompleted = task.IsCompleted
            };
        }

        public static List<TaskItemResponse> ToTaskItemResponses(List<TaskItem> tasks)
        {
            return tasks.Select(ToTaskItemResponse).ToList();
        }

        public static ProgressResponse CalculateProgress(List<TaskItem> tasks)
        {
            if (tasks.Count == 0)
            {
                return new ProgressResponse
                {
                    CompletedTasks = 0,
                    TotalTasks = 0,
                    Percentage = 0
                };
            }

            int completed = tasks.Count(t => t.IsCompleted);
            int total = tasks.Count;

            return new ProgressResponse
            {
                CompletedTasks = completed,
                TotalTasks = total,
                Percentage = Math.Round((double)completed / total * 100, 2)
            };
        }
    }
}
