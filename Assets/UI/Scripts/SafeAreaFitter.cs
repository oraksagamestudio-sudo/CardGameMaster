// Assets/UI/Scripts/SafeAreaFitter.cs
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform rect;

    [SerializeField] private UIDynamicLayoutManager nextUILayoutManager;
    [SerializeField] private int nextUIUpdateDelayFrameCount;

    public float marginLeft = 0f;
    public float marginRight = 0f;
    public float marginTop = 0f;
    public float marginBottom = 0f;

    private Rect lastSafeArea;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        rect = GetComponent<RectTransform>();
        ApplySafeArea();

#if UNITY_EDITOR
        EditorApplication.update -= WatchEditorSafeArea;
        EditorApplication.update += WatchEditorSafeArea;
#endif
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= WatchEditorSafeArea;
#endif
    }

#if UNITY_EDITOR
    private void WatchEditorSafeArea()
    {
        if (Application.isPlaying)
            return;

        // Editor에서는 SafeArea를 'Viewport 변할 때만' 갱신
        Rect now = Screen.safeArea;

        if (SafeAreaChanged(now, lastSafeArea))
        {
            ApplySafeArea();
        }
    }
#endif
    private bool SafeAreaChanged(Rect a, Rect b)
    {
        const float threshold = .5f;
        return
            Mathf.Abs(a.x - b.x) > threshold ||
            Mathf.Abs(a.y - b.y) > threshold ||
            Mathf.Abs(a.width - b.width) > threshold ||
            Mathf.Abs(a.height - b.height) > threshold;
    }

    public void Update()
    {
        if (!Application.isPlaying) return;

        Rect now = Screen.safeArea;

        if (SafeAreaChanged(now, lastSafeArea))
        {
            ApplySafeArea();
            
        }
    }

    private void ApplySafeArea()
    {
        //Debug.Log($"1. SafeArea 계산 시작");
        Rect safe = Screen.safeArea;
        lastSafeArea = safe;

        safe.xMin += marginLeft;
        safe.xMax -= marginRight;
        safe.yMin += marginBottom;
        safe.yMax -= marginTop;

        // Canvas 기준 변환
        Canvas canvas = GetComponentInParent<Canvas>();
        //RectTransform canvasRT = canvas.GetComponent<RectTransform>();

        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;

        anchorMin.x /= canvas.pixelRect.width;
        anchorMin.y /= canvas.pixelRect.height;
        anchorMax.x /= canvas.pixelRect.width;
        anchorMax.y /= canvas.pixelRect.height;

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;

        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Debug.Log($"[SafeAreaFitter] Applied safe area: {safe}");

        if (nextUILayoutManager != null)
            StartCoroutine(EnableNextFrame());
    }

    IEnumerator EnableNextFrame()
    {
        yield return null;
        nextUILayoutManager.ApplyLayout(nextUIUpdateDelayFrameCount);
    }
}