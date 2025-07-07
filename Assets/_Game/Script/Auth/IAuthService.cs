public interface IAuthService
{
    bool Authenticate(string email, string password);
    bool Register(string email, string password);
}