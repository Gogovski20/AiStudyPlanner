namespace AiStudyPlanner.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(string username, string email, string password);
        Task<string> LoginAsync(string email, string password);
    }
}
