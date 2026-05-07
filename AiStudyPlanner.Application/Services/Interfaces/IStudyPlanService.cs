using AiStudyPlanner.Domain.Models;

namespace AiStudyPlanner.Application.Services.Interfaces
{
    public interface IStudyPlanService
    {
        Task<ChatHistory> GenerateAndSaveAsync(int userId, string input);
        Task<IEnumerable<ChatHistory>> GetHistoryAsync(int userId);
        Task<ChatHistory?> GetHistoryByIdAsync(int userId, int historyId);
        Task<ChatHistory?> CompleteTaskAsync(int userId, int historyId, Guid taskId);
        Task<ChatHistory?> IncompleteTaskAsync(int userId, int historyId, Guid taskId);
        Task<ChatHistory?> UpdateTaskTitleAsync(
            int userId,
            int historyId,
            Guid taskId,
            string newTitle
        );
        Task<ChatHistory?> DeleteTaskAsync(int userId, int historyId, Guid taskId);
        Task<ChatHistory?> AddTaskAsync(int userId, int historyId, string title);
        Task<bool> DeleteHistoryAsync(int userId, int historyId);
    }
}
