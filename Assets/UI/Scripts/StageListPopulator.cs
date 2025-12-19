// StageListPopulator.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.U2D; // 사용 안 하면 제거
using UnityEngine.SceneManagement;
using System.Threading.Tasks; // 사용 안 하면 제거

public class StageListPopulator : MonoBehaviour
{
    // Freecell 씬으로 전달할 시드값 (static으로 씬 간 공유)
    
    [Header("Scroll & Content")]
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private RectTransform content; // ScrollRect.content와 동일한 객체

    [Header("Item Prefab")]
    [SerializeField] private StageButtonController stageButtonPrefab;

    [Header("Initial Fill")]
    [SerializeField] private uint initialCount = 30;

    private readonly List<StageButtonController> items = new();

    void Start()
    {
        

        if (items.Count == 0 && initialCount > 0)
            RebuildList(initialCount);
    }

    public async void RebuildList(uint count)
    {
        Clear();

        for (uint i = 1; i <= count; i++)
            await AddOne(i);

        // 레이아웃 강제 갱신으로 스냅백 방지
        ForceLayout();
        // 원하면 맨 위 정렬
        if (scroll != null) scroll.verticalNormalizedPosition = 1f;
    }

    public async Task<StageButtonController> AddOne(uint stageNumber)
    {
        var go = Instantiate(stageButtonPrefab, content);
        await go.Setup(stageNumber, OnClickStage);
        items.Add(go);
        return go;
    }

    public void Clear()
    {
        foreach (var it in items)
            if (it != null) Destroy(it.gameObject);
        items.Clear();
    }

    private async void OnClickStage(uint stage)
    {
        Debug.Log($"[StageList] Click: {stage}");
        
        // 시드값 저장 (임시로 스테이지 번호를 시드로 사용)
        FreecellLaunchParams.Seed = stage;
        FreecellLaunchParams.HasSeed = true;
        
        // Freecell 씬으로 이동
        await SceneManager.LoadSceneAsync("Freecell");
    }

    /// 리스트를 갱신하거나 아이템 추가/삭제 직후 호출
    public void ForceLayout()
    {
        Canvas.ForceUpdateCanvases();
        if (content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            Canvas.ForceUpdateCanvases();
        }
    }

    // 특정 스테이지 버튼으로 스크롤 초점 이동 (즉시)
    public void FocusStage(int stage, float align = 1f, float padding = 8f)
    {
        var target = items.Find(x => x != null && x.gameObject.activeInHierarchy && GetStage(x) == stage);
        if (target == null) return;

        ForceLayout(); // 위치 계산 전 최신화
        ScrollToChildImmediate(scroll, target.GetComponent<RectTransform>(), align, padding);
    }

    private int GetStage(StageButtonController sb)
    {
        // StageButton에 현재 stageNumber를 노출하는 getter를 두고 싶다면 추가해서 사용
        // 여기서는 label 텍스트 파싱 대신, 필요 시 StageButton에 public StageNumber {get;}
        // 프로퍼티를 구현해두는 걸 추천.
        return 0; // 필요 시 구현
    }

    // 간단한 즉시 스크롤 (세로 전용)
    private static void ScrollToChildImmediate(ScrollRect scroll, RectTransform target, float align, float padding)
    {
        if (scroll == null || target == null) return;
        var content = scroll.content;
        var viewport = scroll.viewport != null ? scroll.viewport : scroll.GetComponent<RectTransform>();
        if (content == null || viewport == null) return;

        var targetBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(content, target);
        var contentBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(content);
        var viewportBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(content, viewport);

        float ch = contentBounds.size.y;
        float vh = viewportBounds.size.y;
        if (ch <= vh) { scroll.verticalNormalizedPosition = 1f; return; }

        float targetTop = targetBounds.max.y + padding;
        float targetCenter = (targetBounds.min.y + targetBounds.max.y) * 0.5f;
        float anchor = Mathf.Lerp(targetCenter, targetTop, align); // align=1 상단정렬, 0.5 중앙

        float halfView = vh * 0.5f;
        float desiredY = anchor - halfView;
        float scrollable = ch - vh;
        float normY = 1f - Mathf.Clamp01((desiredY - contentBounds.min.y) / scrollable);
        scroll.verticalNormalizedPosition = normY;
    }
}