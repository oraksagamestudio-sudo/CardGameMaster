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
        StartCoroutine(BootFlow());
    }

    private IEnumerator BootFlow()
    {
        
        clientVersionText.text = $"v{Application.version}";
        // 로딩패널 활성화
        loadingPanel.SetActive(true);

        // ## 로딩 시작 ##

        // + bootstrapper 초기화는 첫프레임 끝나야 완료됨.
        yield return SetProgress(0.01f, "boot_init-bootstrapper");
        yield return null; // 한 프레임 대기
        var bootstrapper = Bootstrapper.Instance;

        // + bootstrapper 인터넷 연결 체크 (if no connection, go to offline Mode Scene)
        yield return SetProgress(0.02f, "boot_check-internet");
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
        yield return SetProgress(0.10f, "boot_check-server-status");
        bool serverOk = false;
        yield return bootstrapper.CheckServerStatus((ok) => serverOk = ok);
        if (!serverOk)
        {
            Debug.LogError("[Boot] Server is under maintenance.");
            SceneManager.LoadScene("OfflineMode"); // [서버 점검 중] 오프라인 모드 씬으로 전환
            yield break;
        }

        // + bootstrapper 업데이트 체크 (if need update, go to update scene)
        yield return SetProgress(0.2f, "boot_check-update");
        bool needUpdate = false;
        yield return bootstrapper.CheckForUpdates(Application.version, (need) => { needUpdate = need; });
        if (needUpdate)
        {
            Debug.LogError("[Boot] Update required.");
            SceneManager.LoadScene("Update"); // [업데이트 필요] 업데이트 씬으로 전환
            yield break;
        }

        // + 리소스 무결성 체크
        yield return SetProgress(0.5f, "boot_check-resources");
        bool resourcesOk = false;
        yield return bootstrapper.CheckResourceIntegrity((ok) => resourcesOk = ok);
        if (!resourcesOk)
        {
            Debug.LogError("[Boot] Resource integrity check failed.");
            SceneManager.LoadScene("Update"); // [리소스 무결성 오류] 업데이트
            yield break;
        }
        //TODO [인트로로딩] Bootstrapper 리소스 무결성 체크 필요 (if need update, do update)
        //TODO [인트로로딩] Bootstrapper 필수 데이터 체크 필요 (if no data, do download)
        //TODO [인트로로딩] Bootstrapper 로그인체크 필요(if not logged in, go to login scene)

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
