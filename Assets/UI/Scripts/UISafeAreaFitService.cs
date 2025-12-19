//Assets/UI/Scripts/UISafeAreaFitService.cs
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class UISafeAreaFitService : MonoBehaviour
{
    [Header("References")]
    public RectTransform root;          // SafeArea 적용된 최상위
    public RectTransform topArea;
    public RectTransform infoBar;
    public RectTransform slotArea;
    public RectTransform tableusArea;
    public RectTransform bottomArea;

    [Header("Column Reference (ex: Tableau_1)")]
    public RectTransform sampleColumn;   // 하나의 칼럼 레퍼런스

    [Header("Card")]
    public float cardAspect = 1.5f;      // width * 1.5 = height

    [Header("Bottom Area Height")]
    public float bottomAreaHeight = 250;

    private Vector2 lastScreenSize;
    private float lastRootH;
    private float lastInfoH;
    private float lastSlotH;
    private bool heightInitialized = false;
    private bool layoutDirty = true;
    private bool isApplyingLayout = false;

    private void OnEnable()
    {
        layoutDirty = true;
        Canvas.preWillRenderCanvases -= HandleWillRenderCanvases;
        Canvas.preWillRenderCanvases += HandleWillRenderCanvases;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            InitializeEditorLayout();
            return;
        }
#endif
        ApplyLayout();
    }

    private void OnDisable()
    {
        Canvas.preWillRenderCanvases -= HandleWillRenderCanvases;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (ScreenHasChanged() || UIHeightHasChanged())
        {
            layoutDirty = true;
        }
    }
#endif

    private void OnRectTransformDimensionsChange()
    {
        if (!isApplyingLayout)
        {
            layoutDirty = true;
        }
    }

    private void HandleWillRenderCanvases()
    {
        if (layoutDirty)
        {
            ApplyLayout();
        }
    }

#if UNITY_EDITOR
    private void InitializeEditorLayout()
    {
        if (root == null || sampleColumn == null) return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(root);

        CalculateCardSize();
        // UILayoutBroadcaster.Broadcast();

        LayoutRebuilder.ForceRebuildLayoutImmediate(root);
        layoutDirty = false;
    }
#endif

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
        float curRoot = root.rect.height;
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

    private void CalculateCardSize()
    {
        float cardWidth = sampleColumn.rect.width;
        float cardHeight = cardWidth * cardAspect;

        //CardSizeProvider.Init(cardWidth, cardHeight, cardGap: cardHeight / 3);
    }
    private void ApplyLayout()
    {
        Debug.Log($"3. 기존 레이아웃팅 시작");
        if (isApplyingLayout) return;
        if (root == null || sampleColumn == null) return;

        isApplyingLayout = true;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(root);

        // 1) safe area width 기반
        float rootHeight = root.rect.height;

        // 2) 8등분 → 카드 너비 확정
        CalculateCardSize();
        //UILayoutBroadcaster.Broadcast();

        // 3) SlotArea 높이 = cardHeight
        float slotHeight = slotArea.rect.height;
        float infoBarHeight = infoBar.rect.height;

        // 4) TopArea 높이 = slotHeight + margin
        float topAreaHeight = slotHeight + infoBarHeight;
        
        // 5) BottomArea 높이는 고정 or 옵션값
        // float bottomHeight = 100;

        // 6) 나머지 높이 tableusArea가 가져감 (Dock.Fill)
        float tableusHeight =
            rootHeight - topAreaHeight - bottomAreaHeight;
        //var tableusPaddingHeight = tableusArea.gameObject.GetComponent<LayoutGroup>().padding.vertical;
        // tableusHeight -= tableusPaddingHeight;
        if (tableusHeight < 0) tableusHeight = 0;

        // ---------------------------
        // Apply sizes
        // ---------------------------
        SetHeight(topArea, topAreaHeight);
        SetHeight(slotArea, slotHeight);
        SetHeight(bottomArea, bottomAreaHeight);
        SetHeight(tableusArea, tableusHeight);

        isApplyingLayout = false;
        layoutDirty = false;

    }

    private void SetHeight(RectTransform rt, float h)
    {
        var size = rt.sizeDelta;
        size.y = h;
        rt.sizeDelta = size;
    }

}
