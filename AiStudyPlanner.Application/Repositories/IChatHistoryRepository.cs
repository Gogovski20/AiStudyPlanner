using AiStudyPlanner.Domain.Models;

namespace AiStudyPlanner.Application.Repositories
{
    public interface IChatHistoryRepository
    {
        Task AddAsync(ChatHistory chat);
        Task<ChatHistory?> GetByIdAsync(int id);
        Task<IEnumerable<ChatHistory>> GetByUserIdAsync(int userId);
        Task SaveChangesAsync();
        void Delete(ChatHistory chatHistory);
    }
}
