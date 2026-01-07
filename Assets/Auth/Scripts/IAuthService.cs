using System.Collections;

public interface IAuthService
{
    IEnumerator SignInWithGoogleAsync();
    IEnumerator SignInWithAppleAsync();

    string UserId { get; }
    string DisplayName { get; }
    string Provider { get; }
}