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

        public async Task<ChatHistory?> CompleteTaskAsync(int userId, int historyId, int taskIndex)
        {
            var history = await _chatHistoryRepository.GetByIdAsync(historyId);

            if (history == null || history.UserId != userId)
                return null;

            if (taskIndex < 0 || taskIndex >= history.Tasks.Count)
                throw new ArgumentOutOfRangeException(nameof(taskIndex), "Invalid task index.");

            history.Tasks[taskIndex].IsCompleted = true;

            await _chatHistoryRepository.SaveChangesAsync();

            return history;
        }

        public async Task<ChatHistory?> IncompleteTaskAsync(int userId, int historyId, int taskIndex)
        {
            var history = await _chatHistoryRepository.GetByIdAsync(historyId);

            if (history == null || history.UserId != userId)
                return null;

            if (taskIndex < 0 || taskIndex >= history.Tasks.Count)
                throw new ArgumentOutOfRangeException(nameof(taskIndex), "Invalid task index.");

            history.Tasks[taskIndex].IsCompleted = false;

            await _chatHistoryRepository.SaveChangesAsync();

            return history;
        }
    }
}