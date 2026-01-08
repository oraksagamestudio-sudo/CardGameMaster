// Assets/Auth/Scripts/AuthFacade.cs
using System.Collections;
using System.Threading.Tasks;
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
        // TODO: 실제 구현에서는 PlayerPrefs나 키체인에 저장된 토큰 확인
        // 지금은 "autoLogin" 키가 1이면 무조건 진입하는 형태로 간단하게
        int auto = PlayerPrefs.GetInt("autoLogin", 0);
        if (auto == 0)
            return false;

        // 더미 서비스에선 그냥 성공으로 처리
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

    public static void DisableAutoLogin()
    {
        PlayerPrefs.DeleteKey("autoLogin");
    }

#region New Auth Methods with Callbacks
    public static IEnumerator TryAutoLogin(System.Action<bool> onCompleted)
    {
        Task<bool> loginTask = TryAutoLoginAsync();
        while (!loginTask.IsCompleted)
        {
            yield return null;
        }
        onCompleted?.Invoke(loginTask.Result);
    }
#endregion
}