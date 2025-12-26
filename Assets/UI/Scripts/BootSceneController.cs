using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BootSceneController : MonoBehaviour
{

    [Header("Config")]
    [SerializeField] private float logoTime = 1.0f;

    private void Start()
    {
        StartCoroutine(BootFlow());
    }

    private IEnumerator BootFlow()
    {
        //TODO Bootstrapper 로그인체크 필요(if not logged in, go to login scene)
        yield return new WaitForSeconds(logoTime);
        yield return LoadingTest();
        
    }

    private IEnumerator LoadingTest()
    {

        /*
        // GPT Guide Example Usage
        var model = new LoadingPanelModel
        {
            UseBackground = true,
            ShowSpinner = true,
            ShowProgressText = true,
            OnCompleted = () => { Debug.Log("Loading Done"); }
        };

        GlobalLoadingService.Show(model);

        GlobalLoadingService.SetProgress(0.3f, "Loading Assets...");
        GlobalLoadingService.SetProgress(0.8f, "Almost Done...");
        GlobalLoadingService.Complete();
        */
        var model = new LoadingPanelModel
        {
            UseBackground = true,
            RequireTouchToContinue = false,
            ShowTooltip = true,
            ShowSpinner = true,
            ShowProgressText = true,
            ShowPercentText = true,
            StepDelay = 0.05f
        };

        model.OnCompleted += () =>
        {
            Debug.Log("Loading Completed");
            SceneManager.LoadScene("Lobby");
        };

        model.OnFailed += () =>
        {
            Debug.Log("Loading Failed");
        };

        GlobalLoadingService.Show(model);

        for (int i = 0; i <= 100; i++)
        {
            GlobalLoadingService.SetProgress(i / 100f, $"Loading... {i}/100");
            yield return new WaitForSeconds(model.StepDelay);
        }

        GlobalLoadingService.Complete();
    }

}
