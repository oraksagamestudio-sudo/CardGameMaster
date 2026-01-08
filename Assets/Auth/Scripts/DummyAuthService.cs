using System.Threading.Tasks;
using UnityEngine;

public class DummyAuthService : IAuthService
{
    public string UserId { get; private set; }
    public string DisplayName { get; private set; }
    public string Provider { get; private set; }

    public async Task<bool> SignInWithGoogleAsync()
    {
        Provider = "Google";
        await Task.Delay(1200);
        UserId = "dummy.google.user";
        DisplayName = "Guest";
        Debug.Log("[DummyAuth] Google SignIn success");
        return true;
    }

    public async Task<bool> SignInWithAppleAsync()
    {
#if UNITY_IOS
        Provider = "Apple";
        await Task.Delay(1200);
        UserId = "dummy.apple.user";
        DisplayName = "Guest";
        Debug.Log("[DummyAuth] Apple SignIn success");
        return true;
#else
        Debug.LogWarning("[DummyAuth] Apple SignIn only on iOS");
        return false;
#endif
    }
}