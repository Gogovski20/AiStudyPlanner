namespace AiStudyPlanner.API.Contracts.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public AuthUserResponse User { get; set; } = new();
    }
}
