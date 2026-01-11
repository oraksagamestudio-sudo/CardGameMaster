using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Localization.Settings;

public class BootSceneController : MonoBehaviour
{

    [Header("Config")]
    [SerializeField] private Image introGameTitle;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingMessage;
    [SerializeField] private Slider loadingProgressBar;
    [SerializeField] private TextMeshProUGUI loadingProgressValue;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject donePanel;
    [SerializeField] private TextMeshProUGUI clientVersionText;
    [SerializeField] private bool autoLogin = true;
    [SerializeField] private bool useFastBoot = false;
    [Header("Debug")]
    [SerializeField] private bool skipBootFlow = false;
    [SerializeField] private bool goToLobbyDirectly = false;
    [SerializeField] private bool goToOfflineModeDirectly = false;
    [SerializeField] private bool goToUpdateDirectly = false;
    [SerializeField] private bool showLoginPanelDirectly = false;
    [SerializeField] private bool showDonePanelDirectly = false;
    [SerializeField] private bool stopWhenLoadingComplete = false; // Editor 전용: 로딩 완료 후 멈춤
    [SerializeField] private bool simulateNoInternet = false; // Editor 전용: 인터넷 연결 없음 시뮬레이션
    [SerializeField] private bool simulateServerMaintenance = false; // Editor 전용: 서버 점검 중 시뮬레이션
    [SerializeField] private bool useDonePanelAfterLogin = true; // Editor 전용: 로그인 후 완료 패널 표시

    private void Start()
    {
        clientVersionText.text = $"v{Application.version}";

        StartCoroutine(BootFlow());
    }
    
    private static void SetTitleImage(ref Image imageComponent, string resourcePath)
    {
        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        imageComponent.sprite = sprite;
        imageComponent.rectTransform.sizeDelta = new Vector2(sprite.rect.size.x, sprite.rect.size.y);
    }

    private IEnumerator BootFlow()
    {


        // + bootstrapper 초기화는 첫프레임 끝나야 완료됨.
        yield return null; // 한 프레임 대기
        var bootstrapper = Bootstrapper.Instance;

        // + 로컬라이제이션 초기화
        yield return bootstrapper.SetLocale();
        var locale = LocalizationSettings.SelectedLocale;
        Debug.Log($"[Boot] Localization initialized. Locale: {locale.Identifier.Code}");

        // + 언어에 따른 인트로 타이틀 표시
        switch (locale.Identifier.Code)
        {
            case "ko":
                SetTitleImage(ref introGameTitle, "Images/intro-title");
                break;
                
            default:
            case "en":
                SetTitleImage(ref introGameTitle, "Images/intro-title_global");
                break;
        }

        introGameTitle.gameObject.SetActive(true);

        // 패널 상태 초기화
        loginPanel.SetActive(false);
        donePanel.SetActive(false);
        loadingPanel.SetActive(true);

        if (skipBootFlow)
        {
            yield return SetProgress(1f, "boot_complete");
            loadingPanel.SetActive(false);
            SceneManager.LoadScene("Lobby");
            yield break;
        }
        if (goToLobbyDirectly)
        {
            yield return SetProgress(1f, "boot_complete");
            loadingPanel.SetActive(false);
            SceneManager.LoadScene("Lobby");
            yield break;
        }
        if (goToOfflineModeDirectly)
        {
            yield return SetProgress(1f, "boot_complete");
            loadingPanel.SetActive(false);
            SceneManager.LoadScene("OfflineMode");
            yield break;
        }
        if (goToUpdateDirectly)
        {
            yield return SetProgress(1f, "boot_complete");
            loadingPanel.SetActive(false);
            SceneManager.LoadScene("Update");
            yield break;
        }
        if (showLoginPanelDirectly)
        {
            yield return SetProgress(1f, "boot_complete");
            loadingPanel.SetActive(false);
            loginPanel.SetActive(true);
            yield break;
        }
        if (showDonePanelDirectly)
        {
            yield return SetProgress(1f, "boot_complete");
            loadingPanel.SetActive(false);
            donePanel.SetActive(true);
            yield break;
        }

        // ## 로딩 시작 ##


        // + bootstrapper 인터넷 연결 체크 (if no connection, go to offline Mode Scene)
        yield return SetProgress(0.1f, "boot_check-internet");
        if (simulateNoInternet)
        {
            Debug.LogError("[Boot] Simulated no internet connection.");
            SceneManager.LoadScene("OfflineMode"); // [연결 없는 경우] 오프라인 모드 씬으로 전환
            yield break;
        }
        bootstrapper.CheckInternetConnection();
        float internetCheckTimeout = 5f;
        float elapsed = 0f;
        while (!bootstrapper.IsInternetCheckDone && elapsed < internetCheckTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (!bootstrapper.IsInternetCheckDone || Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("[Boot] Internet check failed or timed out.");
            SceneManager.LoadScene("OfflineMode"); // [연결 없는 경우] 오프라인 모드 씬으로 전환
            yield break;
        }


        // + bootstrapper 서버 상태 체크 (if server maintenance, go to offline Mode Scene)
        yield return SetProgress(0.3f, "boot_check-server-status");
        if (simulateServerMaintenance)
        {
            Debug.LogError("[Boot] Simulated server maintenance.");
            SceneManager.LoadScene("OfflineMode"); // [서버 점검 중/운영 종료 시] 오프라인 모드 씬으로 전환
            yield break;
        }
        bool serverOk = false;
        yield return bootstrapper.CheckServerStatus((ok) => serverOk = ok);
        if (!serverOk)
        {
            Debug.LogError("[Boot] Server is under maintenance.");
            SceneManager.LoadScene("OfflineMode"); // [서버 점검 중/운영 종료 시] 오프라인 모드 씬으로 전환
            yield break;
        }


        // + bootstrapper 업데이트 체크 (if need update, go to update scene)
        yield return SetProgress(0.4f, "boot_check-update");
        bool needUpdate = false;
        yield return bootstrapper.CheckForUpdates(Application.version, (need) => { needUpdate = need; });
        if (needUpdate)
        {
            Debug.LogError("[Boot] Update required.");
            SceneManager.LoadScene("Update"); // [업데이트 필요] 업데이트 씬으로 전환
            yield break;
        }


        // // + 리소스 무결성 체크
        // //TODO 어드레서블스 적용 후 구현
        // /*
        //     - 필수 리소스 파일 존재 여부
        //     - 리소스 파일 해시값 검증
        // */
        // yield return SetProgress(0.5f, "boot_check-resources");
        // bool resourcesOk = false;
        // yield return bootstrapper.CheckResourceIntegrity((ok) => resourcesOk = ok);
        // if (!resourcesOk)
        // {
        //     Debug.LogError("[Boot] Resource integrity check failed.");
        //     //TODO 리소스 결점 수정(대체) 작업 (첫 실행 시 추가 카다록에 의한 다운로드 부분) 어드레서블스
        //     yield break;
        // }


        // + 자동로그인 시도 (if auto login fail, show login panel)
        yield return SetProgress(0.8f, "boot_auto-login");
        bool loginOk = false;
        
        // 자동로그인 설정 확인(1이면 로그인 이력 있음)
        bool autoLoginInPref = PlayerPrefs.GetInt("autoLogin", 0) == 1;
        
        if (autoLogin && autoLoginInPref)
        {
            AuthFacade.TryAutoLogin((ok) =>
            {
                loginOk = ok;
            });
        }
        if (!loginOk)
        {
            // 자동로그인 실패 시 로그인 패널 표시
            loadingPanel.SetActive(false);
            loginPanel.SetActive(true);
            yield break; // 로그인 후 다시 부트플로우 시작하도록
        }


        // + 사용자 데이터 로드
        /*
            - 사용자 프로필
            - 인벤토리
            - 상품 구매 이력
            - 게임 설정
            - 저장된 게임 상태
        */
        yield return SetProgress(0.9f, "boot_load-user-data");
        bool userDataOk = false;
        yield return bootstrapper.LoadUserData((ok) => userDataOk = ok);
        if (!userDataOk)
        {
            Debug.LogError("[Boot] Failed to load user data.");
            SceneManager.LoadScene("OfflineMode"); // [사용자 데이터 로드 실패] 오
            yield break;
        }


        // + Heartbeat 서비스 시작
        yield return SetProgress(0.95f, "boot_start-heartbeat");
        // TODO: Heartbeat 세션 토큰 얻어오기
        // string heartbeatToken = string.Empty;
        // yield return HeartbeatService.Instance.GetSessionToken((token) => {
        //     Debug.Log("[Boot] Heartbeat Session Token Acquired.");
        //     heartbeatToken = token;
        // });
        // HeartbeatService.Instance.StartService(heartbeatToken);
        Debug.Log("[Boot] Heartbeat Service Started.");

        // + 로딩완료
        yield return SetProgress(1f, "boot_complete");

        // ## 로딩 끝 ##
#if UNITY_EDITOR
        Debug.Log("[Boot] Boot flow completed.");
        if (stopWhenLoadingComplete)
            yield break;
#endif

        loadingPanel.SetActive(false);

        if (useDonePanelAfterLogin)
        {
            donePanel.SetActive(true);
            yield break;
        }
        SceneManager.LoadScene("Lobby");


    }

    // private IEnumerator LoadingTest()
    // {

    //     /*
    //     // GPT Guide Example Usage
    //     var model = new LoadingPanelModel
    //     {
    //         UseBackground = true,
    //         ShowSpinner = true,
    //         ShowProgressText = true,
    //         OnCompleted = () => { Debug.Log("Loading Done"); }
    //     };

    //     GlobalLoadingService.Show(model);

    //     GlobalLoadingService.SetProgress(0.3f, "Loading Assets...");
    //     GlobalLoadingService.SetProgress(0.8f, "Almost Done...");
    //     GlobalLoadingService.Complete();
    //     */
    //     var model = new LoadingPanelModel
    //     {
    //         UseBackground = true,
    //         RequireTouchToContinue = false,
    //         ShowTooltip = true,
    //         ShowSpinner = true,
    //         ShowProgressText = true,
    //         ShowPercentText = true,
    //         StepDelay = 0.05f
    //     };

    //     model.OnCompleted += () =>
    //     {
    //         Debug.Log("Loading Completed");
    //         if(autoLogin)
    //             SceneManager.LoadScene("Lobby");
    //     };

    //     model.OnFailed += () =>
    //     {
    //         Debug.Log("Loading Failed");
    //     };

    //     GlobalLoadingService.Show(model);

    //     for (int i = 0; i <= 100; i++)
    //     {
    //         GlobalLoadingService.SetProgress(i / 100f, $"Loading... {i}/100");
    //         yield return new WaitForSeconds(model.StepDelay);
    //     }

    //     GlobalLoadingService.Complete();
    // }

    // public bool LoginCheck()
    // {
    //     //TODO 로그인 체크
    //     return true;
    // }
    private IEnumerator SetProgress(float progress, string messagekey)
    {
        loadingProgressBar.value = progress;
        loadingProgressValue.text = $"{(int)(progress * 100)}%";
        loadingMessage.text = L.S("boot", messagekey);
        yield return new WaitForSeconds(useFastBoot ? 0.5f : 1f);
    }
}
