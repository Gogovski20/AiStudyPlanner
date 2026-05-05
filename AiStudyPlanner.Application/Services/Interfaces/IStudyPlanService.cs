using AiStudyPlanner.Domain.Models;

namespace AiStudyPlanner.Application.Services.Interfaces
{
    public interface IStudyPlanService
    {
        Task<ChatHistory> GenerateAndSaveAsync(int userId, string input);
        Task<IEnumerable<ChatHistory>> GetHistoryAsync(int userId);
        Task<ChatHistory?> GetHistoryByIdAsync(int userId, int historyId);
        Task<ChatHistory?> CompleteTaskAsync(int userId, int historyId, int taskIndex);
        Task<ChatHistory?> IncompleteTaskAsync(int userId, int historyId, int taskIndex);
    }
}
