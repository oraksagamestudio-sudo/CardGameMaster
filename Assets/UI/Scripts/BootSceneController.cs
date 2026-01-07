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

    private void Start()
    {
        StartCoroutine(BootFlow());
    }

    private IEnumerator BootFlow()
    {
        float progress = 0f;
        float step = 0.01f;
        clientVersionText.text = $"v{Application.version}";
        // 로딩패널 활성화
        loadingPanel.SetActive(true);
        
        //bootstrapper 초기화는 첫프레임 끝나야 완료됨.
        SetProgress(progress += step, "boot_init-bootstrapper");
        yield return null;
        var bootstrapper = Bootstrapper.Instance;
        // TODO 로딩오브젝트 초기화 후 순차적으로 로딩 진행하는 워크플로우로 변경 
        // List<LoadingAction> loadingActions; // LoadingAction[int progressWeight, UnityAction action, string messageKey]
        // loadingActions = new List<LoadingAction>()
        // {
        //     new LoadingAction(10, bootstrapper.CheckInternetConnection, "boot_chk-internet-connection"),
        //     new LoadingAction(20, bootstrapper.CheckServerStatus, "boot_chk-server-status"),
        //     new LoadingAction(20, bootstrapper.CheckForUpdates, "boot_chk-for-updates"),
        //     new LoadingAction(20, bootstrapper.VerifyResourceIntegrity, "boot_verify-resource-integrity"),
        //     new LoadingAction(20, bootstrapper.CheckEssentialData, "boot_chk-essential-data"),
        //     new LoadingAction(10, bootstrapper.CheckLoginStatus, "boot_chk-login-status"),
        // };

        // LoadingAction.ExecuteAll(loadingActions, loadingPanel);

        //TODO [인트로로딩] Bootstrapper 인터넷 연결 체크 필요 (if no connection, go to error scene)
        //introPanel.SetProgress(0.1f, "boot_chk-internet-connection");
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
            yield break;
        }
        //TODO [인트로로딩] Bootstrapper 서버 상태 체크 필요 (if server maintenance, go to error scene)
        //TODO [인트로로딩] Bootstrapper 업데이트 체크 필요 (if need update, go to update scene)
        //TODO [인트로로딩] Bootstrapper 리소스 무결성 체크 필요 (if need update, do update)
        //TODO [인트로로딩] Bootstrapper 필수 데이터 체크 필요 (if no data, do download)
        //TODO [인트로로딩] Bootstrapper 로그인체크 필요(if not logged in, go to login scene)


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
    private void SetProgress(float progress, string messagekey)
    {
        loadingProgressBar.value = progress;
        loadingProgressValue.text = $"{(int)(progress * 100)}%";
        loadingMessage.text = L.S("boot", messagekey);
    }
}
