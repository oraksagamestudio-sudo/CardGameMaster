using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Bootstrapper : MonoBehaviour
{
    public static Bootstrapper Instance { get; private set; }

    // 글로벌 부트 진행 이벤트 (씬 간 공유)
    public static float LastProgress { get; private set; }
    public static string LastMessage { get; private set; }

    // 부트씬이 참고할 상태
    public bool IsInitialized { get; private set; } = false;
    public string NextSceneName { get; private set; } = "Lobby"; // 기본값 하나 잡아두기

    [Header("App Settings")]
    [SerializeField] private AppConfig appConfig;
    [SerializeField] private bool useStop = false;
    [SerializeField] private bool slowLoadingMode = false;

    [Header("Localization")]
    [SerializeField] private string bootProgressTable = "boot"; // String Table 이름 (예: "boot")

    [Header("Server Catalog")]
    [SerializeField] private HeartbeatPolicyRegistry heartbeatRegistry;
    [SerializeField] private string heartbeatUri = "/api/heartbeat.php";
    [SerializeField] private string heartbeatToken = ""; // 로그인 후 갱신 주입 가능
    [SerializeField] private string stageListRemoteUri = "/api/fetch_stage_seeds.php";
    [Header("UI References")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressMessage;
    [SerializeField] private TextMeshProUGUI progressText;

    private IRemoteContentService remote;



    private void Start()
    {
        // 싱글턴 처리
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        /*
        
        await ReportKeyAsync(0.05f, "boot_init_remote");

        // 1) 원격 콘텐츠 초기화
        remote = new AddressablesRemoteContentService();
        await remote.InitializeAsync(appConfig != null ? appConfig.remoteCatalogUrl : "");
        await ReportKeyAsync(0.20f, "boot_remote_ready");

        await ReportKeyAsync(0.22f, "boot_loading_localization");
        // 2) Localization 초기화
        await LocalizationSettings.InitializationOperation.Task;
        await ReportKeyAsync(0.32f, "boot_localization_ready");

        await ReportKeyAsync(0.34f, "boot_selecting_locale");
        await SetLocaleAsync(); // ✅ 새 비동기 로케일 설정 호출
        await ReportKeyAsync(0.38f, "boot_locale_selected", LocalizationSettings.SelectedLocale?.Identifier.Code ?? "unknown");

        await ReportKeyAsync(0.40f, "boot_attempt_autologin");
        // 3) 로그인 준비 & 자동로그인 시도
        bool autoOk = false;
        try
        {
            autoOk = await AuthFacade.TryAutoLoginAsync();
            if (autoOk)
                await ReportKeyAsync(0.48f, "boot_autologin_success");
            else
                await ReportKeyAsync(0.48f, "boot_autologin_skipped");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Bootstrapper] AutoLogin failed: {e.Message}");
            autoOk = false;
            await ReportKeyAsync(0.48f, "boot_autologin_failed");
        }

        //Stage seed 
        await ReportKeyAsync(0.54f, "boot_applying_stage_seeds");
        //TODO: local stage seeds 확인하기
        //TODO: 서버에서 stage seeds 가져오기
        string fetchStageSeedsUrl = appConfig.serverUrl + stageListRemoteUri;

        await ReportKeyAsync(0.55f, "boot_updating_stage_seeds");
        //TODO: if 로컬 스테이지시드 없으면 서버 스테이지 시드 저장
        
        //TODO: if 로컬스테이지시드 있는데 버전 안맞으면 최신 버전으로 저장

        await ReportKeyAsync(0.58f, "boot_stage_seeds_applied");

        await ReportKeyAsync(0.60f, "boot_config_heartbeat");
        // HeartbeatService 오브젝트가 없으면 생성
        if (HeartbeatService.Instance == null)
        {
            var go = new GameObject("HeartbeatService");
            go.AddComponent<HeartbeatService>();
        }

        string heartbeatUrl = appConfig.serverUrl + heartbeatUri;
        HeartbeatService.Instance.Configure(heartbeatRegistry, heartbeatToken, heartbeatUrl);
        await ReportKeyAsync(0.70f, "boot_heartbeat_ready");

        await ReportKeyAsync(0.75f, "boot_selecting_next_scene");
        // 여기서 다음 씬만 결정해둔다
        NextSceneName = autoOk ? "Lobby" : "Login";
        await ReportKeyAsync(0.85f, "boot_next_scene", NextSceneName);

        await ReportKeyAsync(0.92f, "boot_complete");
        // “난 부팅 끝남” 표시
        IsInitialized = true;
        await ReportKeyAsync(1.00f, "boot_ready");

        // 여기서 바로 씬 안 넘긴다. (← 핵심 변경)
        if (!useStop)
        {
            // 부트씬이 있으면 부트씬이 넘기고,
            // 부트씬이 아예 없으면 여기서 바로 로딩
            // (fallback)
            // Boot씬이 이미 로드된 상태라면 재로딩하지 않음
            var active = SceneManager.GetActiveScene();
            if (!useStop && active.name == "Boot")
            {
                var nextScene = NextSceneName;
                await SceneManager.LoadSceneAsync(nextScene);
            }
        }
        */
    }

    private void UpdateProgress(float p01, string message)
    {
        LastProgress = Mathf.Clamp01(p01);
        LastMessage = message;

        if (progressBar != null)
            progressBar.value = LastProgress;

        if (progressText != null)
            progressText.text = $"{(int)(LastProgress * 100)}%";

        if (progressMessage != null)
            progressMessage.text = LastMessage;
    }

    /// <summary>
    /// String Table(bootProgressTable)에서 key를 찾아 로컬라이즈 후 Report로 전달.
    /// Smart String 인수를 args로 넘길 수 있음.
    /// </summary>
    private async Task ReportKeyAsync(float p01, string key, params object[] args)
    {
        string msg = key;
        AsyncOperationHandle<string> handle = default;
        try
        {
            // 로컬라이제이션 시스템 준비 대기
            await LocalizationSettings.InitializationOperation.Task;

            // Smart String 인수 처리 (Addressables AsyncOperationHandle<string> -> await handle.Task)
            if (args != null && args.Length > 0)
                handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(bootProgressTable, key, args);
            else
                handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(bootProgressTable, key);

            await handle.Task;
            msg = handle.Result;
            if (string.IsNullOrEmpty(msg))
                msg = key; // 키 미존재/빈 문자열일 때 안전망
        }
        catch
        {
            // 실패 시 key 그대로 표시 (개발 단계 안전망)
            msg = key;
        }

        UpdateProgress(p01, msg);

        if (handle.IsValid())
        {
            await DelayedRelease(handle);
        }
    }

    private async Task DelayedRelease(AsyncOperationHandle handle)
    {

        if (slowLoadingMode)
            await Task.Delay(500);
        else
            await Task.Delay(30);
        if (handle.IsValid())
            Addressables.Release(handle);
    }

    private async Task SetLocaleAsync()
    {
        await LocalizationSettings.InitializationOperation.Task;

        // ✅ Unity 내부 Locale 자동 선택이 끝날 때까지 한 프레임 대기
        await Task.Yield();

        if (appConfig.defaultLanguage == SystemLanguage.Unknown)
        {
            // 이 시점에는 SelectedLocale이 실제 반영된 상태
            var currentLocale = LocalizationSettings.SelectedLocale;
            string code = currentLocale?.Identifier.Code ?? "en";

            if (code.StartsWith("ko"))
                appConfig.defaultLanguage = SystemLanguage.Korean;
            else
                appConfig.defaultLanguage = SystemLanguage.English;

            SaveAppConfig();
            Debug.Log($"[Bootstrapper] First-run detected. Language auto-saved: {appConfig.defaultLanguage}");
            return;
        }

        // 이후 실행 시 AppConfig 기반 강제 적용
        var available = LocalizationSettings.AvailableLocales;
        Locale targetLocale = FindLocaleByLanguage(available, appConfig.defaultLanguage);

        if (targetLocale != null && LocalizationSettings.SelectedLocale != targetLocale)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
            Debug.Log($"[Bootstrapper] Locale fixed from AppConfig: {targetLocale.Identifier.Code}");
        }
    }

    /// <summary>
    /// Helper to find locale by SystemLanguage
    /// </summary>
    private Locale FindLocaleByLanguage(ILocalesProvider available, SystemLanguage lang)
    {
        if (available == null) return null;

        // ✅ SystemLanguage를 ISO 코드(ko, en 등)로 변환
        string isoCode = GetIsoCodeFromSystemLanguage(lang);

        var locales = available.Locales;
        if (locales == null || locales.Count == 0) return null;

        // 1️⃣ 정확히 일치하는 코드 우선
        Locale exact = locales.FirstOrDefault(l =>
            l.Identifier.Code.Equals(isoCode, StringComparison.OrdinalIgnoreCase) ||
            l.Identifier.CultureInfo?.TwoLetterISOLanguageName == isoCode);
        if (exact != null)
            return exact;

        // 2️⃣ 접두어 일치 허용 (ko-KR vs ko)
        return locales.FirstOrDefault(l =>
            l.Identifier.Code.StartsWith(isoCode, StringComparison.OrdinalIgnoreCase) ||
            (l.Identifier.CultureInfo != null &&
             l.Identifier.CultureInfo.TwoLetterISOLanguageName == isoCode));
    }

    /// <summary>
    /// SystemLanguage → ISO 639-1 code 변환 (ko, en 등)
    /// </summary>
    private string GetIsoCodeFromSystemLanguage(SystemLanguage lang)
    {
        switch (lang)
        {
            case SystemLanguage.Korean: return "ko";
            case SystemLanguage.English: return "en";
            case SystemLanguage.Japanese: return "ja";
            case SystemLanguage.ChineseSimplified:
            case SystemLanguage.ChineseTraditional:
            case SystemLanguage.Chinese: return "zh";
            case SystemLanguage.French: return "fr";
            case SystemLanguage.German: return "de";
            case SystemLanguage.Spanish: return "es";
            case SystemLanguage.Italian: return "it";
            case SystemLanguage.Russian: return "ru";
            default:
                return lang.ToString().Substring(0, 2).ToLowerInvariant();
        }
    }

    private void SaveAppConfig()
    {
#if UNITY_EDITOR
        // UnityEditor.EditorUtility.SetDirty(appConfig);
        // UnityEditor.AssetDatabase.SaveAssets();
#else
    // 런타임 환경에서는 PlayerPrefs로 보관 (ScriptableObject는 빌드 후 저장 불가)
    PlayerPrefs.SetString("AppConfig.Language", appConfig.defaultLanguage.ToString());
    PlayerPrefs.Save();
#endif
    }


    #region New Boot Flow
    public bool IsInternetCheckDone { get; private set; } = false;
    public void CheckInternetConnection()
    {
        StartCoroutine(CheckInternetConnectionRoutine());
    }
    private IEnumerator CheckInternetConnectionRoutine()
    {

        var reachability = Application.internetReachability;
#if UNITY_ANDROID || UNITY_IOS
        bool isWifi = reachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        bool isMobile = reachability == NetworkReachability.ReachableViaCarrierDataNetwork;
        if (isWifi || isMobile)
            Debug.Log($"[Bootstrapper] Network: {(isWifi ? "WiFi" : "Mobile")}");
        else
            Debug.LogWarning("[Bootstrapper] Network: NotReachable");
#else
        if (reachability == NetworkReachability.NotReachable)
            Debug.LogWarning("[Bootstrapper] Network: NotReachable");
#endif
        IsInternetCheckDone = true;
        yield break;
    }
    #endregion

}
