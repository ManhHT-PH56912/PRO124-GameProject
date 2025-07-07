public interface IInputValidator
{
    bool IsValidEmail(string email, out string errorMessage);
    bool IsValidPassword(string password, out string errorMessage);
}