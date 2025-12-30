using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BootSceneController : MonoBehaviour
{

    [Header("Config")]
    [SerializeField] private LogoController logoPanel;
    [SerializeField] private float logoTime = 1.0f;
    [SerializeField] private IntroController introPanel;
    [SerializeField]  private bool autoLogin = true;

    private void Start()
    {
        StartCoroutine(BootFlow(logoTime));
    }

    private IEnumerator BootFlow(float logoTime)
    {
        // TODO 로고 표시
        yield return logoPanel.ShowLogoAsync(logoTime);

        //bootstrapper 초기화는 알아서 됨.
        var bootstrapper = Bootstrapper.Instance;
        // TODO 인트로로딩 화면 표시
        introPanel.gameObject.SetActive(true);

        //TODO [인트로로딩] Bootstrapper 인터넷 연결 체크 필요 (if no connection, go to error scene)
        introPanel.SetProgress(0.1f, "boot_chk-internet-connection");
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

}
