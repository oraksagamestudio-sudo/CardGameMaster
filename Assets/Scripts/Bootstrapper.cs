using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using System.Collections;
using UnityEngine.Networking;
using Unity.Services.Core;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Services.Authentication;

public class Bootstrapper : MonoBehaviour
{
    private sealed class PermissiveCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
    public static Bootstrapper Instance { get; private set; }

    // 부트씬이 참고할 상태
    public bool IsInitialized { get; private set; } = false;

    [Header("App Settings")]
    [SerializeField] private AppConfig appConfig;

    [Header("Server Catalog")]
    [SerializeField] private HeartbeatPolicyRegistry heartbeatRegistry;
    [SerializeField] private string serverLiveCheckUri;
    [SerializeField] private string clientVersionCheckUri;
    [SerializeField] private string heartbeatUri;
    private string heartbeatToken = ""; // 로그인 후 갱신 주입 가능
    [SerializeField] private string stageListRemoteUri = "/api/fetch_stage_seeds.php";
    
    private string lastestClientVersion;

    public string HeartbeatURL { get => appConfig.serverUrl + heartbeatUri; }


    private async void Start()
    {
        // 싱글턴 처리
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        await UnityServices.InitializeAsync();
        Debug.Log("[Bootstrapper] Unity Services Initialized");
        IsInitialized = true;

        Debug.Log("[Bootstrapper] Bootstrapper started");
    }

    public IEnumerator SetLocale()
    {
        yield return LocalizationSettings.InitializationOperation;

        if (string.IsNullOrEmpty(appConfig.defaultLocaleCode))
        {
            // SelectedLocale은 초기화 타이밍에 따라 변경될 수 있으므로
            // 에디터에서는 선택된 Locale을 우선 사용하고, 그 외에는 시스템 언어를 사용합니다.
            string detectedCode = null;

#if UNITY_EDITOR
            var simLang = GetEditorSimulatorSystemLanguage();
            if (simLang.HasValue)
            {
                detectedCode = GetIsoCodeFromSystemLanguage(simLang.Value);
                Debug.Log($"[Bootstrapper] Editor simulator language detected: {simLang.Value}");
            }
            if (string.IsNullOrEmpty(detectedCode))
            {
                var selected = LocalizationSettings.SelectedLocale;
                if (selected != null)
                    detectedCode = selected.Identifier.Code;
            }
#endif
            if (string.IsNullOrEmpty(detectedCode))
                detectedCode = GetIsoCodeFromSystemLanguage(Application.systemLanguage);

            if (string.IsNullOrEmpty(detectedCode))
            {
                var currentLocale = LocalizationSettings.SelectedLocale;
                detectedCode = currentLocale?.Identifier.Code;
            }

            // Legacy 마이그레이션은 감지 실패 시에만 사용
            if (string.IsNullOrEmpty(detectedCode) && appConfig.defaultLanguage != SystemLanguage.Unknown)
            {
                detectedCode = GetIsoCodeFromSystemLanguage(appConfig.defaultLanguage);
                Debug.Log($"[Bootstrapper] Legacy language migrated: {detectedCode}");
            }

            if (string.IsNullOrEmpty(detectedCode))
                detectedCode = "en";

            appConfig.defaultLocaleCode = detectedCode;
            appConfig.defaultLanguage = SystemLanguage.Unknown;


            SaveAppConfig();
            Debug.Log($"[Bootstrapper] First-run detected. Locale auto-saved: {appConfig.defaultLocaleCode}");
            yield break;
        }

        // 이후 실행 시 AppConfig 기반 강제 적용
        var available = LocalizationSettings.AvailableLocales;
        Locale targetLocale = FindLocaleByCode(available, appConfig.defaultLocaleCode);

        if (targetLocale != null && LocalizationSettings.SelectedLocale != targetLocale)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
            Debug.Log($"[Bootstrapper] Locale fixed from AppConfig: {targetLocale.Identifier.Code}");
        }
    }

    /// <summary>
    /// Helper to find locale by SystemLanguage
    /// </summary>
    private Locale FindLocaleByCode(ILocalesProvider available, string localeCode)
    {
        if (available == null) return null;

        if (string.IsNullOrEmpty(localeCode)) return null;

        var locales = available.Locales;
        if (locales == null || locales.Count == 0) return null;

        // 1️⃣ 정확히 일치하는 코드 우선
        Locale exact = locales.FirstOrDefault(l =>
            l.Identifier.Code.Equals(localeCode, StringComparison.OrdinalIgnoreCase) ||
            l.Identifier.CultureInfo?.TwoLetterISOLanguageName.Equals(localeCode, StringComparison.OrdinalIgnoreCase) == true);
        if (exact != null)
            return exact;

        // 2️⃣ 접두어 일치 허용 (ko-KR vs ko)
        return locales.FirstOrDefault(l =>
            l.Identifier.Code.StartsWith(localeCode, StringComparison.OrdinalIgnoreCase) ||
            (l.Identifier.CultureInfo != null &&
             l.Identifier.CultureInfo.TwoLetterISOLanguageName.Equals(localeCode, StringComparison.OrdinalIgnoreCase)));
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

#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 Device Simulator 등 시뮬레이터가 제공하는 시스템언어를 리플렉션으로 시도해서 가져옵니다.
    /// 존재하지 않으면 null을 반환합니다.
    /// </summary>
    private SystemLanguage? GetEditorSimulatorSystemLanguage()
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in assemblies)
            {
                foreach (var t in asm.GetTypes())
                {
                    if (!t.Name.ToLowerInvariant().Contains("simul"))
                        continue;

                    // 탐색: static property 또는 field로 언어 정보를 노출하는 경우
                    var prop = t.GetProperty("language") ?? t.GetProperty("Language") ?? t.GetProperty("SimulatedLanguage") ?? t.GetProperty("SystemLanguage");
                    if (prop != null)
                    {
                        object val = null;
                        if (prop.GetMethod.IsStatic)
                            val = prop.GetValue(null);
                        else
                        {
                            // 인스턴스가 필요하면 가능한 static Instance 프로퍼티를 찾아 사용
                            var instProp = t.GetProperty("instance") ?? t.GetProperty("Instance");
                            if (instProp != null)
                            {
                                var inst = instProp.GetValue(null);
                                if (inst != null)
                                    val = prop.GetValue(inst);
                            }
                        }

                        if (val is SystemLanguage sl)
                            return sl;
                        if (val is string s)
                        {
                            if (Enum.TryParse<SystemLanguage>(s, true, out var parsed))
                                return parsed;
                        }
                    }

                    // Locale/Identifier 같은 형태로 보관할 수도 있음
                    var localeProp = t.GetProperty("Locale") ?? t.GetProperty("locale") ?? t.GetProperty("SelectedLocale");
                    if (localeProp != null)
                    {
                        object loc = null;
                        if (localeProp.GetMethod.IsStatic)
                            loc = localeProp.GetValue(null);
                        else
                        {
                            var instProp = t.GetProperty("instance") ?? t.GetProperty("Instance");
                            if (instProp != null)
                            {
                                var inst = instProp.GetValue(null);
                                if (inst != null)
                                    loc = localeProp.GetValue(inst);
                            }
                        }

                        if (loc != null)
                        {
                            var codeProp = loc.GetType().GetProperty("Identifier")?.PropertyType.GetProperty("Code");
                            // 위 방식이 복잡하면 ToString으로 파싱 시도
                            var code = loc.ToString();
                            if (!string.IsNullOrEmpty(code) && code.StartsWith("ko", StringComparison.OrdinalIgnoreCase))
                                return SystemLanguage.Korean;
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // 안전하게 무시
        }

        return null;
    }
#endif

    private void SaveAppConfig()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(appConfig);
        UnityEditor.AssetDatabase.SaveAssets();
#else
        // 런타임 환경에서는 PlayerPrefs로 보관 (ScriptableObject는 빌드 후 저장 불가)
        PlayerPrefs.SetString("AppConfig.Locale", appConfig.defaultLocaleCode ?? "");
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
    
    
    public IEnumerator CheckServerStatus(Action<bool> onCompleted)
    {
        bool serverOk = false;
        // 서버 상태 체크 로직 (예: HTTP 요청)
        var serverLiveCheckUrl = appConfig.serverUrl + serverLiveCheckUri + "/"; // http://hyemini.com/orsgs/cgmapi/lc.do로 연결(실제 디렉토리는 lc.do/index.php)
        // if (Uri.TryCreate(hbUrl, UriKind.Absolute, out var hbUri) && hbUri.Scheme == Uri.UriSchemeHttp)
        // {
        //     var builder = new UriBuilder(hbUri)
        //     {
        //         Scheme = Uri.UriSchemeHttps,
        //         Port = hbUri.Port == 80 ? -1 : hbUri.Port
        //     };
        //     hbUrl = builder.Uri.AbsoluteUri;
        //     Debug.LogWarning($"[Bootstrapper] Insecure HTTP blocked; upgrading heartbeat to HTTPS: {hbUrl}");
        // }
        var param = new Dictionary<string, string>()
        {
            {"lang", appConfig.defaultLocaleCode},
            {"appid", Application.identifier}
        };
        var www = UnityWebRequest.Post(serverLiveCheckUrl, param);
        // Debug.Log($"[Bootstrapper] Checking server status: {www.url}");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!string.IsNullOrEmpty(www.error) && www.error.Contains("SSL"))
            {
                Debug.LogWarning($"[Bootstrapper] SSL error on heartbeat; retrying with permissive cert handler: {www.error}");
                www.Dispose();
                www = UnityWebRequest.Post(serverLiveCheckUrl, param);
                www.certificateHandler = new PermissiveCertificateHandler();
                www.disposeCertificateHandlerOnDispose = true;
                yield return www.SendWebRequest();
            }
#endif
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Bootstrapper] Server status check failed: {www.error}\n{www.downloadHandler.text}");
                serverOk = false;
            }
        }
        if (www.result == UnityWebRequest.Result.Success && www.responseCode != 200)
        {
            Debug.LogError($"[Bootstrapper] Server status check failed: HTTP {www.responseCode}");
            serverOk = false;
        }
        else if (www.result == UnityWebRequest.Result.Success)
        {
            // 서버 응답 처리
            Debug.Log("[Bootstrapper] Server status check success");
            serverOk = true; // 실제 응답에 따라 결정
        }
        
        onCompleted?.Invoke(serverOk);
        yield break;
    }

    private static string GetURLQueryString(Dictionary<string, string> param)
    {
        var queryString = new StringBuilder();
        queryString.Append("?");
        int count = 0;
        foreach (var p in param)
        {
            if (count > 0) queryString.Append("&");
            queryString.Append(p.Key);
            queryString.Append("=");
            queryString.Append(UnityWebRequest.EscapeURL(p.Value));
            count++;
        }
        return queryString.ToString();
    }
    public IEnumerator CheckForUpdates(Action<bool> onCompleted)
    {
        bool needUpdate = false;
        // 업데이트 체크 로직 (예: HTTP 요청)
        var param = new Dictionary<string, string>()
        {
            #if UNITY_IOS
            {"os","ios"},
            #else
            {"os", "android"},
            #endif
            {"appid", Application.identifier}  
        };
        
        var updateUrl = $"{appConfig.serverUrl}{clientVersionCheckUri}{GetURLQueryString(param)}";
        var www = UnityWebRequest.Get(updateUrl);
        
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[Bootstrapper] Update check failed: {www.error}");
            needUpdate = false; // 오류 시 업데이트 불필요로 간주
        }
        else if( www.responseCode != 200)
        {
            Debug.LogError($"[Bootstrapper] Update check failed: HTTP {www.responseCode}");
            needUpdate = false; // 오류 시 업데이트 불필요로 간주
        }
        else
        {
            // 서버 응답 처리
            // 예: {"need_update": true}
            var json = www.downloadHandler.text;
            try
            {
                var response = JsonUtility.FromJson<UpdateCheckResponse>(json);
                needUpdate = response.lastest_version != Application.version;
                Debug.Log($"[Bootstrapper] Update check success: current version={Application.version}, lastest version={response.lastest_version}\n{json}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Bootstrapper] Update check parse failed: {e.Message}");
                needUpdate = false; // 파싱 오류 시 업데이트 불필요로 간주
            }
        }
        
        onCompleted?.Invoke(needUpdate);
        yield break;
    }

    // public IEnumerator CheckResourceIntegrity(Action<bool> onCompleted)
    // {
    //     bool integrityOk = false;
    //     // 리소스 무결성 체크 로직 (예: 파일 해시 체크)
    //     var integrityUrl = appConfig.serverUrl + "/api/check_integrity.php";
    //     var www = UnityWebRequest.Get(integrityUrl);
    //     yield return www.SendWebRequest();

    //     if (www.result != UnityWebRequest.Result.Success)
    //     {
    //         Debug.LogError($"[Bootstrapper] Resource integrity check failed: {www.error}");
    //         integrityOk = false; // 오류 시 무결성 불필요로 간주
    //     }
    //     else if (www.responseCode != 200)
    //     {
    //         Debug.LogError($"[Bootstrapper] Resource integrity check failed: HTTP {www.responseCode}");
    //         integrityOk = false; // 오류 시 무결성 불필요로 간주
    //     }
    //     else
    //     {
    //         // 서버 응답 처리
    //         var json = www.downloadHandler.text;
    //         try
    //         {
    //             var response = JsonUtility.FromJson<IntegrityCheckResponse>(json);
    //             integrityOk = response.integrity_ok;
    //             Debug.Log($"[Bootstrapper] Resource integrity check success: integrity_ok={integrityOk}");
    //         }
    //         catch (Exception e)
    //         {
    //             Debug.LogError($"[Bootstrapper] Resource integrity check parse failed: {e.Message}");
    //             integrityOk = false; // 파싱 오류 시 무결성 불필요로 간주
    //         }
    //     }

    //     onCompleted?.Invoke(integrityOk);
    //     yield break;
    // }

    public IEnumerator LoadUserData(Action<bool> onCompleted)
    {
        bool userDataOk = false;
        // 사용자 데이터 로드 로직 (예: HTTP 요청)
        //TODO: 유저 데이터 받아오는 워크플로우 구현하기
        yield return new WaitForSeconds(1.0f); // 더미 대기
        userDataOk = true; // 성공으로 간주


        onCompleted?.Invoke(userDataOk);
        yield break;
    }
#endregion


#region Login Functions

    public IEnumerator StartGuestMode()
    {
        yield return AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("Guest UID: " + AuthenticationService.Instance.PlayerId);

        var uid = AuthenticationService.Instance.PlayerId;
        var accessToken = AuthenticationService.Instance.AccessToken;

        //TODO: 서버에 플레이어uid/accessToken 보내서 없으면 새로 유저정보 생성하고 넘어가기
        //있으면 그대로 진행

        PlayerPrefs.SetInt("autoLogin", 1);
        PlayerPrefs.Save();

        //TODO: 다시 부트플로우 시작시켜야함
    }
    #endregion
}

internal class IntegrityCheckResponse
{
    public bool integrity_ok;
}

internal class UpdateCheckResponse
{
    public string lastest_version;
}
