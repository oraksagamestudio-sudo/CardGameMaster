//Assets/UI/Scripts/FreecellClassicLayoutManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// 프리셀 레이아웃 관련 매니저
/// </summary>
[ExecuteAlways]
public class FreecellClassicLayoutManager : UIDynamicLayoutManager
{
    public static FreecellClassicLayoutManager Instance { get; private set; }

    [Header("References (SafeAreaRoot 내부)")]
    public RectTransform rootContainer;
    public RectTransform topArea;
    public RectTransform infoBar;
    public RectTransform slotArea;
    public RectTransform tableausArea;
    public RectTransform bottomArea;

    [Header("Card Settings")]
    public GameObject cardPrefab;
    public float cardAspect = 1.5f;     // width * 1.5 = height
    public float defaultGapDivisor = 3f;
    public float gapScaleStep = 0.1f;
    public Transform generationTarget;   // 초기 생성 위치
    // ======= ▼ READONLY ▼ ========
    public Vector2 CardSize { get; private set; }
    public float CardWidth => CardSize.x;
    public float CardHeight => CardSize.y;
    // =============================

    [Header("Slot References")]
    public GridLayoutGroup freecellGrid;     // 2x2
    public RectTransform tempSlot;
    public GridLayoutGroup foundationGrid;   // 4x1

    [Header("Tableau References")]
    public List<RectTransform> tableaus = new();  // 8개

    [Header("DragLayer")]
    public RectTransform DragLayer;



    private Vector2 lastScreenSize;
    private bool applying = false;

    private float lastRootH;
    private float lastInfoH;
    private float lastSlotH;
    private bool heightInitialized = false;

    /* ===========================================================================
       LIFE CYCLE
       ===========================================================================*/
    //     private void OnEnable()
    //     {
    //         layoutDirty = true;
    // #if UNITY_EDITOR
    //         EditorApplication.update -= WatchEditorReloaded;
    //         EditorApplication.update += WatchEditorReloaded;
    // #endif
    //     }

    //     void OnDisable()
    //     {
    // #if UNITY_EDITOR
    //         EditorApplication.update -= WatchEditorReloaded;
    // #endif
    //     }

    // #if UNITY_EDITOR
    //     private void WatchEditorReloaded()
    //     {
    //         //Debug.Log("1: ?");
    //         if (ScreenSizeChanged())
    //             layoutDirty = true;

    //         if (layoutDirty)
    //         {
    //             layoutDirty = false;
    //             StartCoroutine(ApplyLayoutNextFrame());
    //         }
    //     }
    // #endif

    public void Awake()
    {
        IsApplied = false;
        Instance = this;
        CalculateCardSize();
    }
    public void OnEnable()
    {
        IsApplied = false;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            InitializeEditorLayout();
            return;
        }
#endif
        CalculateLayoutComponents();
    }


#if UNITY_EDITOR
    private void Update()
    {
        if (ScreenHasChanged() || UIHeightHasChanged())
        {
            IsApplied = false;
        }
    }

    private void InitializeEditorLayout()
    {
        if (rootContainer == null || tableaus[0] == null) return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootContainer);

        CalculateCardSize();

        LayoutRebuilder.ForceRebuildLayoutImmediate(rootContainer);
        IsApplied = true;
    }
#endif

    private void OnRectTransformDimensionsChange()
    {
        if (!applying)
        {
            IsApplied = false;
        }
    }

    protected override void HandleWillRenderCanvases()
    {
        if (!IsApplied)
        {
            CalculateLayoutComponents();
        }
    }
    protected void CalculateCardSize()
    {
        float cardWidth = tableaus[0].rect.width;
        float cardHeight = cardWidth * cardAspect;
        CardSize = new(cardWidth, cardHeight);
    }
    /* ===========================================================================
       MARK DIRTY
       ===========================================================================*/
    private bool ScreenHasChanged()
    {
        Vector2 current = new(Screen.width, Screen.height);
        if (current != lastScreenSize)
        {
            lastScreenSize = current;
            return true;
        }
        return false;
    }

    private bool UIHeightHasChanged()
    {
        float curRoot = rootContainer.rect.height;
        float curInfo = infoBar.rect.height;
        float curSlot = slotArea.rect.height;

        if (!heightInitialized)
        {
            lastRootH = curRoot;
            lastInfoH = curInfo;
            lastSlotH = curSlot;
            heightInitialized = true;
            return false;
        }

        bool changed = (lastRootH != curRoot) ||
                       (lastInfoH != curInfo) ||
                       (lastSlotH != curSlot);

        if (changed)
        {
            lastRootH = curRoot;
            lastInfoH = curInfo;
            lastSlotH = curSlot;
        }

        return changed;
    }

    /* ===========================================================================
       CORE LAYOUT
       ===========================================================================*/

    protected override void CalculateLayoutComponents()
    {
        // Debug.Log($"2. 슬롯 레이아웃 계산 시작");
        if (applying) return;
        if (rootContainer == null || tableaus[0] == null) return;

        applying = true;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootContainer);

        // 1) safe area width 기반
        float rootHeight = rootContainer.rect.height;

        // 2) 8등분 → 카드 너비 확정 update this.CardSize
        CalculateCardSize();

        // 3) 슬롯사이즈 계산 및 적용
        ApplySlotSizes();
        
        var slotAreaSize = slotArea.sizeDelta;
        slotAreaSize.x = rootContainer.rect.width;
        slotArea.sizeDelta = slotAreaSize;

        if (tempSlot != null) // 프리셀클래식에서는 TempSlot 사용하지 않음
            tempSlot.gameObject.SetActive(false);

        // 4) SlotArea 높이 = cardHeight
        float slotHeight = slotArea.rect.height;
        float infoBarHeight = infoBar.rect.height;

        // 5) TopArea 높이 = slotHeight + margin
        float topAreaHeight = slotHeight + infoBarHeight;

        // 6) 나머지 높이 tableusArea가 가져감 (Dock.Fill)
        float tableusHeight =
            rootHeight - topAreaHeight - bottomArea.rect.height;
        if (tableusHeight < 0) tableusHeight = 0;

        // 7) ui높이 적용
        SetHeight(topArea, topAreaHeight);
        SetHeight(slotArea, slotHeight);
        //SetHeight(bottomArea, bottomArea.rect.height);
        SetHeight(tableausArea, tableusHeight);

        // 8) 테이블로 카드 재정렬
        ApplyTableauLayout();

        applying = false;
        IsApplied = true;

    }


    /* ===========================================================================
       SLOT AREA 계산
       ===========================================================================*/

    private void ApplySlotSizes()
    {
        // Freecell 2x2
        if (freecellGrid != null)
        {
            freecellGrid.cellSize = CardSize;
            foreach (var freecell in freecellGrid.GetComponentsInChildren<RectTransform>())
            {
                freecell.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CardWidth);
                freecell.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CardHeight);
            }
        }

        // TempCell
        if (tempSlot != null && CardSize != null)
        {
            tempSlot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CardWidth);
            tempSlot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CardHeight);
        }

        // Foundation 4x1
        if (foundationGrid != null)
        {
            foundationGrid.cellSize = CardSize;
            foreach (var foundation in foundationGrid.GetComponentsInChildren<RectTransform>())
            {
                foundation.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CardWidth);
                foundation.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CardHeight);
            }
        }



        foreach (var tableau in tableaus) {
            if (tableau == null) continue;
            tableau.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CardHeight);
        }
    }


    /* ===========================================================================
       TABLEAU 동적 GAP 계산 + 적용
       ===========================================================================*/

    public void ApplyTableauLayout()
    {
        float containerHeight = tableausArea.rect.height;

        foreach (var tableau in tableaus)
        {
            if (tableau == null) continue;

            float gap = CalculateDynamicGap(tableau, containerHeight);

            // 테블로 내 카드 정렬
            ApplyTableauColumnLayout(tableau, gap);
        }
    }

    /** 테블로에 들어간 카드 개수를 기반으로 gap 계산 */
    private float CalculateDynamicGap(RectTransform tableau, float containerHeight)
    {
        int cardCount = tableau.childCount;
        return CalculateDynamicGap(tableau, containerHeight, cardCount);
    }

    /** 테블로에 들어갈 카드 개수를 지정하여 gap 계산 */
    private float CalculateDynamicGap(RectTransform tableau, float containerHeight, int cardCount)
    {
        if (cardCount <= 1)
            return CardHeight / defaultGapDivisor;

        float scale = 1f;

        while (true)
        {
            float gap = CardHeight / (defaultGapDivisor * scale);

            float totalHeight = CardHeight + gap * (cardCount - 1);

            if (totalHeight <= containerHeight)
                return gap;

            scale += gapScaleStep;
        }
    }

    /** 실제 테블로 내부 카드 정렬 수행 */
    private void ApplyTableauColumnLayout(RectTransform tableau, float gap)
    {
        int count = tableau.childCount;

        for (int i = 0; i < count; i++)
        {
            RectTransform card = tableau.GetChild(i) as RectTransform;
            if (card == null) continue;

            float y = -gap * i;

            card.anchorMin = new Vector2(0.5f, 1);
            card.anchorMax = new Vector2(0.5f, 1);
            card.pivot = new Vector2(0.5f, 1);

            card.anchoredPosition = new Vector2(0, y);
            card.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CardWidth);
            card.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CardHeight);
        }
    }

    private void SetHeight(RectTransform rt, float h)
    {
        var size = rt.sizeDelta;
        size.y = h;
        rt.sizeDelta = size;
    }

    public bool IsInsideTableausArea(RectTransform cardRT)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            tableausArea,
            RectTransformUtility.WorldToScreenPoint(null, cardRT.position),
            null
        );
    }

    public SlotController GetNearestTableau(RectTransform cardRT)
    {
        Vector2 cardPoint = GetTopCenter(cardRT);

        SlotController best = null;
        float bestDist = float.MaxValue;

        foreach (var tableauRT in tableaus)
        {
            if (tableauRT == null) continue;

            Vector2 slotPoint = GetTopCenter(tableauRT);
            float d = Vector2.Distance(cardPoint, slotPoint);

            if (d < bestDist)
            {
                bestDist = d;
                best = tableauRT.GetComponent<SlotController>();
            }
        }

        return best;
    }

    private Vector2 GetTopCenter(RectTransform rt)
    {
        Vector3[] c = new Vector3[4];
        rt.GetWorldCorners(c);
        // c[1] = 좌상단, c[2] = 우상단
        return (Vector2)((c[1] + c[2]) * 0.5f);
    }


    public void UpdateAllSlots()
    {
        foreach (var slot in SlotManager.Instance.AllSlots)
            UpdateLayout(slot.transform);
    }


    public void UpdateLayout(Transform slot)
    {
        int count = slot.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform card = slot.GetChild(i);
            RectTransform rt = card.GetComponent<RectTransform>();

            rt.anchoredPosition = GetLocalPosition(slot, i);
            rt.localRotation = Quaternion.identity;
        }
    }

    private Vector2 GetPositionForIndex(Transform slot, int index)
    {
        RectTransform slotRT = slot as RectTransform;
        
        // ApplyTableauColumnLayout과 동일한 방식으로 계산
        // pivot이 (0.5, 1)이고 anchor가 (0.5, 1)이므로 상단 기준으로 아래로 내려가는 형태
        // 동적 gap 계산 사용 - index는 추가될 카드의 인덱스이므로, 현재 카드 개수는 index
        // gap 계산 시에는 추가될 카드까지 고려하여 index + 1 사용
        float containerHeight = tableausArea != null ? tableausArea.rect.height : slotRT.rect.height;
        int currentCardCount = slotRT.childCount;
        int totalCardCount = Mathf.Max(currentCardCount, index + 1); // 추가될 카드까지 고려
        float gap = CalculateDynamicGap(slotRT, containerHeight, totalCardCount);
        
        // ApplyTableauColumnLayout과 동일: y = -gap * index
        float y = -gap * index;
        
        return new Vector2(0, y);
    }

    public Vector2 GetLocalPosition(Transform slot, int index)
    {
        // 레이아웃이 적용되지 않았다면 먼저 계산하여 정확한 위치 계산
        if (!IsApplied)
        {
            Canvas.ForceUpdateCanvases();
            CalculateLayoutComponents();
        }

        if (slot.GetComponent<TableauController>() != null)
            return GetPositionForIndex(slot, index);
        return Vector2.zero;
    }

    // ============================================================
    // ★ 메인 드롭 판정 함수 (가중치 기반)
    // ============================================================
    public SlotController GetBestSlot(RectTransform cardRT)
    {
        SlotController best = null;
        float bestScore = -999999f;

        foreach (var slot in SlotManager.Instance.AllSlots)
        {
            RectTransform slotRT = slot.transform as RectTransform;

            // 교차 면적
            float overlap = ComputeOverlapArea(cardRT, slotRT);

            // 거리 (카드 중심 → 슬롯 중심)
            float distance = Vector2.Distance(GetRectCenter(cardRT), GetRectCenter(slotRT));

            // 가중치 score (면적 + 거리 보정)
            float score = overlap - distance * 0.15f;
            // distance penalty 0.15f는 UX적으로 가장 안정적임

            if (score > bestScore)
            {
                bestScore = score;
                best = slot;
            }
        }

        // 완전히 안 겹친 경우는 null
        return bestScore > 0 ? best : null;
    }

    // ============================================================
    // 교차 면적 계산
    // ============================================================
    private float ComputeOverlapArea(RectTransform a, RectTransform b)
    {
        Rect ra = GetWorldRect(a);
        Rect rb = GetWorldRect(b);

        float x = Mathf.Max(0, Mathf.Min(ra.xMax, rb.xMax) - Mathf.Max(ra.xMin, rb.xMin));
        float y = Mathf.Max(0, Mathf.Min(ra.yMax, rb.yMax) - Mathf.Max(ra.yMin, rb.yMin));

        return x * y;
    }

    // ============================================================
    // RectTransform → 월드 Rect 변환
    // ============================================================
    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        float xMin = corners[0].x;
        float xMax = corners[2].x;
        float yMin = corners[0].y;
        float yMax = corners[2].y;

        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    private Vector2 GetRectCenter(RectTransform rt)
    {
        Vector3[] c = new Vector3[4];
        rt.GetWorldCorners(c);
        return (Vector2)((c[0] + c[2]) * 0.5f);
    }
}