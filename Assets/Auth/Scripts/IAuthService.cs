using System.Threading.Tasks;

public interface IAuthService
{
    Task<bool> SignInWithGoogleAsync();
    Task<bool> SignInWithAppleAsync();

    string UserId { get; }
    string DisplayName { get; }
    string Provider { get; }
}