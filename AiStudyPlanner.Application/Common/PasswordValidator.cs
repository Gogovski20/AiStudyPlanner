namespace AiStudyPlanner.Application.Common
{
    public class PasswordValidator
    {
        public static bool IsValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < 8)
                return false;

            bool hasUppercase = password.Any(char.IsUpper);
            bool hasLowercase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUppercase && hasLowercase && hasDigit && hasSpecial;
        }

        public static string GetRequirementsMessage()
        {
            return "Password must be at least 8 characters long and contain uppercase, lowercase, number, and special character.";
        }
    }
}
