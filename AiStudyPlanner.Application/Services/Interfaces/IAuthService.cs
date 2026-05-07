using AiStudyPlanner.Application.Models;

namespace AiStudyPlanner.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(string username, string email, string password);
        Task<AuthResult> LoginAsync(string email, string password);
    }
}
