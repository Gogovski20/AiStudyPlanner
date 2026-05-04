using AiStudyPlanner.Domain.Models;

namespace AiStudyPlanner.Application.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsAsync(string email);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
