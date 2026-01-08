using UnityEngine;

[System.Serializable]
public class HeartbeatScenePolicy
{
    [Tooltip("씬 이름 (Build Settings에 등록된 이름과 동일)")]
    public string sceneName;

    [Tooltip("이 씬에서 하트비트 활성화 여부")]
    public bool enabled = true;

    [Tooltip("기본 간격(초)")]
    [Min(1f)] public float intervalSeconds = 15f;

    [Tooltip("지터(±초). 0이면 지터 없음")]
    [Min(0f)] public float jitterSeconds = 2f;

    [Tooltip("씬 진입 시 즉시 1회 전송할지")]
    public bool fireImmediatelyOnEnter = true;
}