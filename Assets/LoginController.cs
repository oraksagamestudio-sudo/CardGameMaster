using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    [SerializeField] private Button googleLoginButton;
    [SerializeField] private Button appleLoginButton;
    [SerializeField] private Button guestModeButton;

    private void OnEnable()
    {
        if (googleLoginButton != null)
            googleLoginButton.onClick.AddListener(OnGoogleLoginClicked);
        if (appleLoginButton != null)
            appleLoginButton.onClick.AddListener(OnAppleLoginClicked);
        if (guestModeButton != null)
            guestModeButton.onClick.AddListener(OnGuestModeClicked);
    }

    private void OnDisable()
    {
        if (googleLoginButton != null)
            googleLoginButton.onClick.RemoveListener(OnGoogleLoginClicked);
        if (appleLoginButton != null)
            appleLoginButton.onClick.RemoveListener(OnAppleLoginClicked);
        if (guestModeButton != null)
            guestModeButton.onClick.RemoveListener(OnGuestModeClicked);
    }

    private void OnGoogleLoginClicked()
    {
        Debug.Log("Google Login Clicked");
        // Implement Google login logic here
    }
    private void OnAppleLoginClicked()
    {
        Debug.Log("Apple Login Clicked");
        // Implement Apple login logic here
    }
    private async void OnGuestModeClicked()
    {
        Debug.Log("Guest Mode Clicked");
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("Guest UID: " + AuthenticationService.Instance.PlayerId);
        
        var uid = AuthenticationService.Instance.PlayerId;
        var accessToken = AuthenticationService.Instance.AccessToken;

        //TODO: 서버에 플레이어uid/accessToken 보내서 없으면 새로 유저정보 생성하고 넘어가기
        //있으면 그대로 진행

        PlayerPrefs.SetInt("autoLogin", 1);
        PlayerPrefs.Save();

        //TODO: 다시 부트플로우 시작시켜야함

    }
}
