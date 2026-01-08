using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Localization.Settings;

public class BootSceneController : MonoBehaviour
{

    [Header("Config")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingMessage;
    [SerializeField] private Slider loadingProgressBar;
    [SerializeField] private TextMeshProUGUI loadingProgressValue;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject donePanel;
    [SerializeField] private TextMeshProUGUI clientVersionText;
    [SerializeField] private bool autoLogin = true;
    [SerializeField] private bool useFastBoot = false;
    

    private void Start()
    {
        clientVersionText.text = $"v{Application.version}";

        StartCoroutine(BootFlow());
    }

    private IEnumerator BootFlow()
    {
        // 로딩패널 활성화
        loadingPanel.SetActive(true);


        // ## 로딩 시작 ##


        // + bootstrapper 초기화는 첫프레임 끝나야 완료됨.
        yield return SetProgress(0.1f, "boot_init-bootstrapper");
        yield return null; // 한 프레임 대기
        var bootstrapper = Bootstrapper.Instance;


        // + bootstrapper 인터넷 연결 체크 (if no connection, go to offline Mode Scene)
        yield return SetProgress(0.2f, "boot_check-internet");
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
        bool serverOk = false;
        yield return bootstrapper.CheckServerStatus((ok) => serverOk = ok);
        if (!serverOk)
        {
            Debug.LogError("[Boot] Server is under maintenance.");
            SceneManager.LoadScene("OfflineMode"); // [서버 점검 중/운영 종료 시] 오프라인 모드 씬으로 전환
            yield break;
        }
        yield return SetProgress(0.35f, "boot_start-heartbeat");
        Debug.Log("[Boot] Heartbeat Service Started.");
        // HeartbeatService.Instance.StartService(bootstrapper.AppConfig.serverUrl);


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


        // + 리소스 무결성 체크
        /*
            - 필수 리소스 파일 존재 여부
            - 리소스 파일 해시값 검증
        */
        yield return SetProgress(0.5f, "boot_check-resources");
        bool resourcesOk = false;
        yield return bootstrapper.CheckResourceIntegrity((ok) => resourcesOk = ok);
        if (!resourcesOk)
        {
            Debug.LogError("[Boot] Resource integrity check failed.");
            SceneManager.LoadScene("Update"); // [리소스 무결성 오류] 업데이트
            yield break;
        }


        // + 로컬라이제이션 초기화
        yield return SetProgress(0.7f, "boot_init-localization");
        yield return LocalizationSettings.InitializationOperation;
        Debug.Log("[Boot] Localization initialized.");


        // + 자동로그인 시도 (if auto login fail, show login panel)
        yield return SetProgress(0.8f, "boot_auto-login");
        bool loginOk = false;
        if (autoLogin)
        {
            yield return AuthFacade.TryAutoLogin((ok) => loginOk = ok);
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

        // + 로딩완료
        yield return SetProgress(1f, "boot_complete");

        // ## 로딩 끝 ##

        loadingPanel.SetActive(false);
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
