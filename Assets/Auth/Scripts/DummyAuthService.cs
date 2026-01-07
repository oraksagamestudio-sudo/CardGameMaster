using System.Collections;
using UnityEngine;

public class DummyAuthService : IAuthService
{
    private static readonly WaitForSeconds _waitForSeconds1_2 = new(1.2f);

    public string UserId { get; private set; }
    public string DisplayName { get; private set; }
    public string Provider { get; private set; }

    public IEnumerator SignInWithGoogleAsync()
    {
        Provider = "Google";
        yield return _waitForSeconds1_2;
        UserId = "dummy.google.user";
        DisplayName = "Guest";
        Debug.Log("[DummyAuth] Google SignIn success");
        yield return true;
    }

    public IEnumerator SignInWithAppleAsync()
    {
#if UNITY_IOS
        Provider = "Apple";
        yield return _waitForSeconds1_2;
        UserId = "dummy.apple.user";
        DisplayName = "Guest";
        Debug.Log("[DummyAuth] Apple SignIn success");
        yield return true;
#else
        Debug.LogWarning("[DummyAuth] Apple SignIn only on iOS");
        yield return false;
#endif
    }
}