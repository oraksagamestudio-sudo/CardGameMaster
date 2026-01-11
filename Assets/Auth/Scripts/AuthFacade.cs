// Assets/Auth/Scripts/AuthFacade.cs
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

public static class AuthFacade
{
    // 지금은 더미
    private static IAuthService _service = new DummyAuthService();

    public static IAuthService Service => _service;

    /// <summary>
    /// 앱 시작 시 자동로그인 시도.
    /// 저장돼 있는 토큰이 없으면 false.
    /// </summary>
    public static async Task<bool> TryAutoLoginAsync()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("[AuthFacade] Already signed in.");
            return true;
        }
        
        bool ok = await _service.SignInWithGoogleAsync(); 
        return ok;
    }

    /// <summary>
    /// 로그인에 성공했을 때 호출해서 다음부터 자동로그인 하도록
    /// </summary>
    public static void EnableAutoLogin()
    {
        PlayerPrefs.SetInt("autoLogin", 1);
        PlayerPrefs.Save();
    }


    #region New Auth Methods with Callbacks
    
    public static void TryAutoLogin(System.Action<bool> onCompleted)
    {
        bool isSignedIn = AuthenticationService.Instance.IsSignedIn;
        if (isSignedIn)
            Debug.Log("[AuthFacade] Already signed in.");
        var uid = AuthenticationService.Instance.PlayerId;
        var aceessToken = AuthenticationService.Instance.AccessToken;
        // TODO: 서버에서 유저 로그인 성공여부만 받아오기
        onCompleted?.Invoke(isSignedIn);
    }
#endregion
}