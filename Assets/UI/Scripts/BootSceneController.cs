using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BootSceneController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject logoPanel;
    [SerializeField] private GameObject loadingPanel;

    [Header("Loading UI")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;   // "74%" 같은 수치
    [SerializeField] private TextMeshProUGUI progressMessage; // "Initializing..." 같은 메시지(옵션)

    [Header("Config")]
    [SerializeField] private float logoTime = 1.0f;

    private bool _usingBootstrapper;
    private float _lastProgress;
    private bool _gotExternalReport;
    private string _lastMessage;

    private void OnEnable()
    {
        if (Bootstrapper.Instance != null)
        {
            _usingBootstrapper = true;
            //Bootstrapper.OnBootProgress += HandleBootProgress;

            // 혹시 이전 메시지가 누락되었다면 재적용
            //Bootstrapper.FlushPendingReports();

            if (Bootstrapper.LastProgress > 0f || !string.IsNullOrEmpty(Bootstrapper.LastMessage))
            {
                _gotExternalReport = true;
                _lastProgress = Bootstrapper.LastProgress;
                _lastMessage = Bootstrapper.LastMessage;
                UpdateProgress(_lastProgress, _lastMessage);
            }
        }
    }

    private void OnDisable()
    {
        if (_usingBootstrapper)
        {
            //Bootstrapper.OnBootProgress -= HandleBootProgress;
            _usingBootstrapper = false;
        }
    }

    private void Start()
    {
        // 처음엔 로고만 보이게
        if (logoPanel != null) logoPanel.SetActive(true);
        if (loadingPanel != null) loadingPanel.SetActive(false);
        
        StartCoroutine(BootFlow());
    }

    private IEnumerator BootFlow()
    {
        yield return new WaitForSeconds(logoTime);
        logoPanel?.SetActive(false);
        loadingPanel?.SetActive(true);

        if (Bootstrapper.Instance != null)
        {
            // 부트스트랩 초기화 대기
            while (!Bootstrapper.Instance.IsInitialized)
                yield return null;

            // 로딩 완료 상태만 표시
            UpdateProgress(1f, "Ready");
            yield break;
        }

        // Fallback만 유지
        yield return StartCoroutine(DoInitializationFallback());
        SceneManager.LoadScene("MainScene");
    }

    private IEnumerator DoInitializationFallback()
    {
        // 간단한 가짜 진행
        float t = 0f;
        while (t < 0.6f)
        {
            t += Time.deltaTime;
            UpdateProgress(Mathf.Clamp01(t), "Initializing...");
            yield return null;
        }
        UpdateProgress(0.85f, "Loading scene...");
        yield return null;
    }

    private void HandleBootProgress(float value01, string message)
    {
        _gotExternalReport = true;
        _lastProgress = Mathf.Max(_lastProgress, value01); // 되돌림 방지
        _lastMessage  = string.IsNullOrEmpty(message) ? _lastMessage : message;
        UpdateProgress(_lastProgress, _lastMessage);
    }

    private void UpdateProgress(float value01, string message = null)
    {
        if (progressBar != null) progressBar.value = value01;

        if (progressText != null)
        {
            progressText.text = Mathf.RoundToInt(value01 * 100f) + "%";
        }

        if (progressMessage != null)
        {
            if (!string.IsNullOrEmpty(message))
                progressMessage.text = message;
            else if (!string.IsNullOrEmpty(_lastMessage))
                progressMessage.text = _lastMessage;
        }
    }
}