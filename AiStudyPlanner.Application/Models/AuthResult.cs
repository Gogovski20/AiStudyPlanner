using AiStudyPlanner.Domain.Models;

namespace AiStudyPlanner.Application.Models
{
    public class AuthResult
    {
        public string Token { get; set; } = string.Empty;
        public User User { get; set; } = null!;
    }
}
