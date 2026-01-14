using UnityEngine;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    [SerializeField] private Button googleLoginButton;
    [SerializeField] private Button appleLoginButton;
    [SerializeField] private Button guestModeButton;

    private void OnEnable()
    {
        appleLoginButton.gameObject.SetActive(false);

        if (googleLoginButton != null)
            googleLoginButton.onClick.AddListener(OnGoogleLoginClicked);
#if UNITY_IOS
        appleLoginButton.gameObject.SetActive(true);
        if (appleLoginButton != null)
            appleLoginButton.onClick.AddListener(OnAppleLoginClicked);
#endif
        if (guestModeButton != null)
            guestModeButton.onClick.AddListener(OnGuestModeClicked);
    }

    private void OnDisable()
    {
        if (googleLoginButton != null)
            googleLoginButton.onClick.RemoveListener(OnGoogleLoginClicked);
#if UNITY_IOS
        if (appleLoginButton != null)
            appleLoginButton.onClick.RemoveListener(OnAppleLoginClicked);
#endif
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

    private void OnGuestModeClicked()
    {
        Debug.Log("Guest Mode Clicked");
        
        StartCoroutine(Bootstrapper.Instance.StartGuestMode());
        
    }
}
