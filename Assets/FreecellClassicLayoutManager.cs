//Assets/UI/Scripts/FreecellClassicLayoutManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class FreecellClassicLayoutManager : UIDynamicLayoutManager
{
    public static FreecellClassicLayoutManager Instance { get; private set; }

    [Header("References (SafeAreaRoot 내부)")]
    [SerializeField] private RectTransform rootContainer;
    [SerializeField] private RectTransform topArea;
    [SerializeField] private RectTransform infoBar;
    [SerializeField] private RectTransform slotArea;
    [SerializeField] private RectTransform tableausArea;
    [SerializeField] private RectTransform bottomArea;

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
    [SerializeField] private GridLayoutGroup freecellGrid;     // 2x2
    [SerializeField] private RectTransform tempSlot;
    [SerializeField] private GridLayoutGroup foundationGrid;   // 4x1

    [Header("Tableau References")]
    [SerializeField] private List<RectTransform> tableaus = new();  // 8개



    private Vector2 lastScreenSize;
    private bool applying = false;

    private float lastRootH;
    private float lastInfoH;
    private float lastSlotH;
    private bool heightInitialized = false;
    private bool layoutDirty;

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
        layoutDirty = true;
        Instance = this;
        CalculateCardSize();
    }
    public void OnEnable()
    {
        layoutDirty = true;
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
            layoutDirty = true;
        }
    }

    private void InitializeEditorLayout()
    {
        if (rootContainer == null || tableaus[0] == null) return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootContainer);

        CalculateCardSize();

        LayoutRebuilder.ForceRebuildLayoutImmediate(rootContainer);
        layoutDirty = false;
    }
#endif

    private void OnRectTransformDimensionsChange()
    {
        if (!applying)
        {
            layoutDirty = true;
        }
    }

    protected override void HandleWillRenderCanvases()
    {
        if (layoutDirty)
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
        Debug.Log($"2. 슬롯 레이아웃 계산 시작");
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
        if(tempSlot)
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
        layoutDirty = false;
        //Canvas.ForceUpdateCanvases();

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
    }


    /* ===========================================================================
       TABLEAU 동적 GAP 계산 + 적용
       ===========================================================================*/

    private void ApplyTableauLayout()
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
}