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
using UnityEngine.Networking;

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
    [SerializeField] private string heartbeatUri = "/api/heartbeat.php";
    [SerializeField] private string heartbeatToken = ""; // 로그인 후 갱신 주입 가능
    [SerializeField] private string stageListRemoteUri = "/api/fetch_stage_seeds.php";
    
    private string lastestClientVersion;



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
        
    }

    // private async Task SetLocaleAsync()
    // {
    //     await LocalizationSettings.InitializationOperation.Task;

    //     // ✅ Unity 내부 Locale 자동 선택이 끝날 때까지 한 프레임 대기
    //     await Task.Yield();

    //     if (appConfig.defaultLanguage == SystemLanguage.Unknown)
    //     {
    //         // 이 시점에는 SelectedLocale이 실제 반영된 상태
    //         var currentLocale = LocalizationSettings.SelectedLocale;
    //         string code = currentLocale?.Identifier.Code ?? "en";

    //         if (code.StartsWith("ko"))
    //             appConfig.defaultLanguage = SystemLanguage.Korean;
    //         else
    //             appConfig.defaultLanguage = SystemLanguage.English;

    //         SaveAppConfig();
    //         Debug.Log($"[Bootstrapper] First-run detected. Language auto-saved: {appConfig.defaultLanguage}");
    //         return;
    //     }

    //     // 이후 실행 시 AppConfig 기반 강제 적용
    //     var available = LocalizationSettings.AvailableLocales;
    //     Locale targetLocale = FindLocaleByLanguage(available, appConfig.defaultLanguage);

    //     if (targetLocale != null && LocalizationSettings.SelectedLocale != targetLocale)
    //     {
    //         LocalizationSettings.SelectedLocale = targetLocale;
    //         Debug.Log($"[Bootstrapper] Locale fixed from AppConfig: {targetLocale.Identifier.Code}");
    //     }
    // }

    // /// <summary>
    // /// Helper to find locale by SystemLanguage
    // /// </summary>
    // private Locale FindLocaleByLanguage(ILocalesProvider available, SystemLanguage lang)
    // {
    //     if (available == null) return null;

    //     // ✅ SystemLanguage를 ISO 코드(ko, en 등)로 변환
    //     string isoCode = GetIsoCodeFromSystemLanguage(lang);

    //     var locales = available.Locales;
    //     if (locales == null || locales.Count == 0) return null;

    //     // 1️⃣ 정확히 일치하는 코드 우선
    //     Locale exact = locales.FirstOrDefault(l =>
    //         l.Identifier.Code.Equals(isoCode, StringComparison.OrdinalIgnoreCase) ||
    //         l.Identifier.CultureInfo?.TwoLetterISOLanguageName == isoCode);
    //     if (exact != null)
    //         return exact;

    //     // 2️⃣ 접두어 일치 허용 (ko-KR vs ko)
    //     return locales.FirstOrDefault(l =>
    //         l.Identifier.Code.StartsWith(isoCode, StringComparison.OrdinalIgnoreCase) ||
    //         (l.Identifier.CultureInfo != null &&
    //          l.Identifier.CultureInfo.TwoLetterISOLanguageName == isoCode));
    // }

//     /// <summary>
//     /// SystemLanguage → ISO 639-1 code 변환 (ko, en 등)
//     /// </summary>
//     private string GetIsoCodeFromSystemLanguage(SystemLanguage lang)
//     {
//         switch (lang)
//         {
//             case SystemLanguage.Korean: return "ko";
//             case SystemLanguage.English: return "en";
//             case SystemLanguage.Japanese: return "ja";
//             case SystemLanguage.ChineseSimplified:
//             case SystemLanguage.ChineseTraditional:
//             case SystemLanguage.Chinese: return "zh";
//             case SystemLanguage.French: return "fr";
//             case SystemLanguage.German: return "de";
//             case SystemLanguage.Spanish: return "es";
//             case SystemLanguage.Italian: return "it";
//             case SystemLanguage.Russian: return "ru";
//             default:
//                 return lang.ToString().Substring(0, 2).ToLowerInvariant();
//         }
//     }

//     private void SaveAppConfig()
//     {
// #if UNITY_EDITOR
//         // UnityEditor.EditorUtility.SetDirty(appConfig);
//         // UnityEditor.AssetDatabase.SaveAssets();
// #else
//     // 런타임 환경에서는 PlayerPrefs로 보관 (ScriptableObject는 빌드 후 저장 불가)
//     PlayerPrefs.SetString("AppConfig.Language", appConfig.defaultLanguage.ToString());
//     PlayerPrefs.Save();
// #endif
//     }


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
        var hbUrl = appConfig.serverUrl + heartbeatUri;
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

        var www = UnityWebRequest.Get(hbUrl);
        // Debug.Log($"[Bootstrapper] Checking server status: {www.url}");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!string.IsNullOrEmpty(www.error) && www.error.Contains("SSL"))
            {
                Debug.LogWarning($"[Bootstrapper] SSL error on heartbeat; retrying with permissive cert handler: {www.error}");
                www.Dispose();
                www = UnityWebRequest.Get(hbUrl);
                www.certificateHandler = new PermissiveCertificateHandler();
                www.disposeCertificateHandlerOnDispose = true;
                yield return www.SendWebRequest();
            }
#endif
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Bootstrapper] Server status check failed: {www.error}");
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


    public IEnumerator CheckForUpdates(string currentVersion, Action<bool> onCompleted)
    {
        bool needUpdate = false;
        // 업데이트 체크 로직 (예: HTTP 요청)
        var updateUrl = appConfig.serverUrl + "/api/check_update.php?version=" + UnityWebRequest.EscapeURL(currentVersion);
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
                needUpdate = response.need_update;
                Debug.Log($"[Bootstrapper] Update check success: need_update={needUpdate}");
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

    public IEnumerator CheckResourceIntegrity(Action<bool> onCompleted)
    {
        bool integrityOk = false;
        // 리소스 무결성 체크 로직 (예: 파일 해시 체크)
        var integrityUrl = appConfig.serverUrl + "/api/check_integrity.php";
        var www = UnityWebRequest.Get(integrityUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[Bootstrapper] Resource integrity check failed: {www.error}");
            integrityOk = false; // 오류 시 무결성 불필요로 간주
        }
        else if (www.responseCode != 200)
        {
            Debug.LogError($"[Bootstrapper] Resource integrity check failed: HTTP {www.responseCode}");
            integrityOk = false; // 오류 시 무결성 불필요로 간주
        }
        else
        {
            // 서버 응답 처리
            var json = www.downloadHandler.text;
            try
            {
                var response = JsonUtility.FromJson<IntegrityCheckResponse>(json);
                integrityOk = response.integrity_ok;
                Debug.Log($"[Bootstrapper] Resource integrity check success: integrity_ok={integrityOk}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Bootstrapper] Resource integrity check parse failed: {e.Message}");
                integrityOk = false; // 파싱 오류 시 무결성 불필요로 간주
            }
        }

        onCompleted?.Invoke(integrityOk);
        yield break;
    }

    public IEnumerator LoadUserData(Action<bool> onCompleted)
    {
        bool userDataOk = false;
        // 사용자 데이터 로드 로직 (예: HTTP 요청)
        yield return new WaitForSeconds(1.0f); // 더미 대기
        userDataOk = true; // 성공으로 간주
        onCompleted?.Invoke(userDataOk);
        yield break;
    }
#endregion

}

internal class IntegrityCheckResponse
{
    public bool integrity_ok;
}

internal class UpdateCheckResponse
{
    public bool need_update;
}
