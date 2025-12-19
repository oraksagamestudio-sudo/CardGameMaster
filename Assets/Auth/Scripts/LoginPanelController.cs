using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class LoginPanelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button buttonGoogle;
    [SerializeField] private Button buttonApple;
    [SerializeField] private GameObject loadingOverlay;
    [SerializeField] private TextMeshProUGUI titleLabel;   // "Welcome" 등

    private IAuthService Auth => AuthServiceLocator.Instance;

    private void Awake()
    {
#if !UNITY_IOS
        if (buttonApple != null) buttonApple.gameObject.SetActive(false);
#endif
        if (loadingOverlay != null) loadingOverlay.SetActive(false);

        if (buttonGoogle != null) buttonGoogle.onClick.AddListener(OnClickGoogle);
        if (buttonApple != null) buttonApple.onClick.AddListener(OnClickApple);
    }

    private async void OnClickGoogle()
    {
        await RunSignIn(async () => await Auth.SignInWithGoogleAsync());
    }

    private async void OnClickApple()
    {
        await RunSignIn(async () => await Auth.SignInWithAppleAsync());
    }

    private async Task RunSignIn(System.Func<Task<bool>> signInFunc)
    {
        SetInteractable(false);
        SetLoading(true);

        bool ok = false;
        string provider = "Unknown";

        try
        {
            ok = await signInFunc();
            provider = Auth.Provider;
        }
        finally
        {
            SetLoading(false);
            SetInteractable(true);
        }

        if (ok)
        {

            // 자동로그인 ON
            AuthFacade.EnableAutoLogin();

            Debug.Log($"[LoginPanel] Signed in with {provider}, user={Auth.UserId}");
            // 로비로
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }
        else
        {
            Debug.LogWarning("[LoginPanel] Sign in failed");
            // TODO: 토스트/다이얼로그 노출
        }
    }

    private void SetLoading(bool on)
    {
        if (loadingOverlay != null) loadingOverlay.SetActive(on);
    }

    private void SetInteractable(bool on)
    {
        if (buttonGoogle != null) buttonGoogle.interactable = on;
        if (buttonApple != null) buttonApple.interactable = on;
    }
}