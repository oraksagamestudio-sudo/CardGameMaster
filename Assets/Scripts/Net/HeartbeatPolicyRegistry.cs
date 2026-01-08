using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "HeartbeatPolicyRegistry", menuName = "Net/Heartbeat Policy Registry")]
public class HeartbeatPolicyRegistry : ScriptableObject
{
    public HeartbeatScenePolicy[] policies;

    [Header("기본값 (매칭 실패 시 사용)")]
    public bool defaultEnabled = false;
    [Min(1f)] public float defaultIntervalSeconds = 20f;
    [Min(0f)] public float defaultJitterSeconds = 0f;
    public bool defaultFireImmediately = false;

    public HeartbeatScenePolicy GetPolicyFor(string sceneName)
    {
        if (policies != null && policies.Length > 0)
        {
            var p = policies.FirstOrDefault(x => x.sceneName == sceneName);
            if (p != null) return p;
        }
        return new HeartbeatScenePolicy
        {
            sceneName = sceneName,
            enabled = defaultEnabled,
            intervalSeconds = defaultIntervalSeconds,
            jitterSeconds = defaultJitterSeconds,
            fireImmediatelyOnEnter = defaultFireImmediately
        };
    }
}