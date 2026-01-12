using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class HeartbeatService : MonoBehaviour
{
    public static HeartbeatService Instance { get; private set; }

    private string authToken = ""; // 필요시 주입

    private HeartbeatPolicyRegistry _registry;
    private Coroutine _loop;
    private int _generation; // 정책 변경 시 루프 무력화용
    private bool _appPaused;

    private float _retryDelay = 1f;
    private float _maxRetryDelay = 30f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    public void Configure(HeartbeatPolicyRegistry registry, string token = null)
    {
        _registry = registry;
        if (!string.IsNullOrEmpty(token)) authToken = token;

        // 현재 씬 기준으로 즉시 정책 적용
        var scene = SceneManager.GetActiveScene().name;
        ApplyPolicyForScene(scene);
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        ApplyPolicyForScene(newScene.name);
    }

    private void ApplyPolicyForScene(string sceneName)
    {
        _generation++;
        if (_loop != null) { StopCoroutine(_loop); _loop = null; }

        if (_registry == null)
        {
            Debug.LogWarning("[Heartbeat] Registry not configured.");
            return;
        }

        var policy = _registry.GetPolicyFor(sceneName);
        if (!policy.enabled)
        {
            Debug.Log($"[Heartbeat] Disabled for scene: {sceneName}");
            return;
        }

        Debug.Log($"[Heartbeat] Enabled for scene: {sceneName}, interval={policy.intervalSeconds}s ±{policy.jitterSeconds}s, fireNow={policy.fireImmediatelyOnEnter}");
        _loop = StartCoroutine(HeartbeatLoop(policy, _generation));
    }

    private IEnumerator HeartbeatLoop(HeartbeatScenePolicy policy, int gen)
    {
        if (policy.fireImmediatelyOnEnter)
        {
            yield return SendOnce(); // 실패해도 다음 주기 진행
        }

        float interval = Mathf.Max(1f, policy.intervalSeconds);
        float jitter = Mathf.Max(0f, policy.jitterSeconds);

        while (gen == _generation)
        {
            // 앱 일시정지 시에는 전송 잠시 중단(원하면 Realtime 유지도 가능)
            if (_appPaused)
            {
                yield return null;
                continue;
            }

            float wait = interval;
            if (jitter > 0f)
            {
                wait += Random.Range(-jitter, jitter);
                wait = Mathf.Max(1f, wait);
            }

            yield return new WaitForSecondsRealtime(wait);

            // 세대 변화(정책 갱신) 시 즉시 종료
            if (gen != _generation) yield break;

            var req = SendOnce();
            while (req.MoveNext()) yield return req.Current;
        }
    }

    private IEnumerator SendOnce()
    {
        _retryDelay = 1f; // reset retry delay at start of SendOnce
        var heartbeatUrl = Bootstrapper.Instance.HeartbeatURL;
        while (true)
        {
            // TODO: 페이로드 설계
            // 예시 페이로드: 프로젝트 상황에 맞게 확장 (userId, deviceId, ping seq 등)
            var payload = new
            {
                ts = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                scene = SceneManager.GetActiveScene().name,
                app = Application.identifier,
                platform = Application.platform.ToString()
            };

            string json = JsonUtility.ToJson(payload);

            using (var req = new UnityWebRequest(heartbeatUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

                if (!string.IsNullOrEmpty(authToken))
                    req.SetRequestHeader("Authorization", $"Bearer {authToken}");

                yield return req.SendWebRequest();

                bool ok = req.result == UnityWebRequest.Result.Success;
                if (!ok)
                {
                    Debug.LogWarning($"[Heartbeat] fail {req.responseCode}: {req.error}");
                    yield return new WaitForSeconds(_retryDelay);
                    _retryDelay = Mathf.Min(_retryDelay * 2f, _maxRetryDelay);
                    continue;
                }
                else
                {
                    _retryDelay = 1f; // reset retry delay on success

                    // 선택: 서버 응답 검사
                    try
                    {
                        var responseText = req.downloadHandler.text;
                        if (!string.IsNullOrEmpty(responseText))
                        {
                            var responseJson = JsonUtility.FromJson<HeartbeatResponse>(responseText);
                            if (responseJson != null && !string.IsNullOrEmpty(responseJson.status))
                            {
                                if (responseJson.status != "ok")
                                {
                                    if (responseJson.status == "update_required")
                                    {
                                        Debug.LogWarning("[Heartbeat] Server indicates update is required.");
                                        // TODO: Hook to UI logic for update required
                                    }
                                    else if (responseJson.status == "outdated")
                                    {
                                        Debug.LogWarning("[Heartbeat] Server indicates client is outdated.");
                                        // TODO: Hook to UI logic for outdated client
                                    }
                                    else if (responseJson.status == "maintenance")
                                    {
                                        Debug.LogWarning("[Heartbeat] Server under maintenance.");
                                        // TODO: Hook to UI logic for maintenance mode
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"[Heartbeat] Unknown server status: {responseJson.status}");
                                    }
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[Heartbeat] Failed to parse server response: {ex.Message}");
                    }
                    break; // exit retry loop on success
                }
            }
        }
    }

    // 앱 포커스/일시정지 대응 (필요에 따라 정책으로 제어 가능)
    private void OnApplicationPause(bool pause) => _appPaused = pause;
    private void OnApplicationFocus(bool focus) => _appPaused = !focus;

    // 외부에서 수동 온/오프 지원
    public void EnableNow(float intervalSeconds = 15f)
    {
        var temp = new HeartbeatScenePolicy
        {
            sceneName = SceneManager.GetActiveScene().name,
            enabled = true,
            intervalSeconds = intervalSeconds,
            jitterSeconds = 0f,
            fireImmediatelyOnEnter = true
        };
        ApplyTempPolicy(temp);
    }

    public void DisableNow()
    {
        _generation++;
        if (_loop != null) StopCoroutine(_loop);
        _loop = null;
    }

    private void ApplyTempPolicy(HeartbeatScenePolicy p)
    {
        _generation++;
        if (_loop != null) StopCoroutine(_loop);
        _loop = StartCoroutine(HeartbeatLoop(p, _generation));
    }

    [System.Serializable]
    private class HeartbeatResponse
    {
        public string status;
    }
}