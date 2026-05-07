using AiStudyPlanner.Application.Repositories;
using AiStudyPlanner.Application.Services.Interfaces;
using AiStudyPlanner.Domain.Models;

namespace AiStudyPlanner.Application.Services.Implementations
{
    public class StudyPlanService : IStudyPlanService
    {
        private readonly IAiService _aiService;
        private readonly IChatHistoryRepository _chatHistoryRepository;

        public StudyPlanService(
            IAiService aiService,
            IChatHistoryRepository chatHistoryRepository)
        {
            _aiService = aiService;
            _chatHistoryRepository = chatHistoryRepository;
        }

        public async Task<ChatHistory> GenerateAndSaveAsync(int userId, string input)
        {
            var result = await _aiService.GetStudyPlanAsync(input);

            var chat = new ChatHistory
            {
                UserId = userId,
                UserInput = input,
                Tasks = result.Tasks,
                EstimatedTime = result.EstimatedTime,
                Priority = result.Priority,
                CreatedAt = DateTime.UtcNow
            };

            await _chatHistoryRepository.AddAsync(chat);
            await _chatHistoryRepository.SaveChangesAsync();

            return chat;
        }

        public async Task<IEnumerable<ChatHistory>> GetHistoryAsync(int userId)
        {
            return await _chatHistoryRepository.GetByUserIdAsync(userId);
        }

        public async Task<ChatHistory?> GetHistoryByIdAsync(int userId, int historyId)
        {
            var history = await _chatHistoryRepository.GetByIdAsync(historyId);

            if (history == null || history.UserId != userId)
                return null;

            return history;
        }

        public async Task<ChatHistory?> CompleteTaskAsync(int userId, int historyId, Guid taskId)
        {
            var history = await _chatHistoryRepository.GetByIdAsync(historyId);

            if (history == null || history.UserId != userId)
                return null;

            var task = history.Tasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            task.IsCompleted = true;

            await _chatHistoryRepository.SaveChangesAsync();

            return history;
        }

        public async Task<ChatHistory?> IncompleteTaskAsync(int userId, int historyId, Guid taskId)
        {
            var history = await _chatHistoryRepository.GetByIdAsync(historyId);

            if (history == null || history.UserId != userId)
                return null;

            var task = history.Tasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            task.IsCompleted = false;

            await _chatHistoryRepository.SaveChangesAsync();

            return history;
        }

        public async Task<ChatHistory?> UpdateTaskTitleAsync(int userId, int historyId, Guid taskId, string newTitle)
        {
            var history = await _chatHistoryRepository.GetByIdAsync(historyId);

            if (history == null || history.UserId != userId)
                return null;

            var task = history.Tasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            if (string.IsNullOrWhiteSpace(newTitle))
                throw new ArgumentException("Task title cannot be empty.", nameof(newTitle));

            task.Title = newTitle.Trim();

            await _chatHistoryRepository.SaveChangesAsync();

            return history;
        }

        public async Task<ChatHistory?> DeleteTaskAsync(int userId, int historyId, Guid taskId)
        {
            var history = await _chatHistoryRepository.GetByIdAsync(historyId);

            if (history == null || history.UserId != userId)
                return null;

            var task = history.Tasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            history.Tasks.Remove(task);

            await _chatHistoryRepository.SaveChangesAsync();

            return history;
        }
    }
}