//Assets/Scripts/CardMotionService.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class CardMotionService : MonoBehaviour
{
    public static CardMotionService Instance;

    [SerializeField] private float defaultDuration = 0.22f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveSfx;

    void Awake()
    {
        Instance = this;
    }

    // -----------------------------------------------------------
    // A. 단일 카드 이동 (부모 변경은 Tween 완료 시 적용)
    // -----------------------------------------------------------
    public Tweener MoveToSlot(RectTransform card, Transform slot, float duration = -1f)
    {
        duration = ResolveDuration(duration);

        // 드래그 레이어 가져오기 (없으면 그대로 이동)
        var dragLayer = FreecellClassicLayoutManager.Instance.DragLayer;
        if (dragLayer != null && card.parent != dragLayer)
        {
            // 드래그 레이어로 먼저 이동
            card.SetParent(dragLayer, worldPositionStays: true);
        }

        SlotController sc = PrepareSlotAndGetTarget(slot, 1, out Vector3 worldTarget);

        // ★ Tween: 월드 좌표로 이동
        Tweener tw = card.DOMove(worldTarget, duration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                // 부모 변경 (목적지 슬롯으로)
                card.SetParent(slot, worldPositionStays: false);

                // SlotController 등록
                sc.AddCard(card.GetComponent<StaticCardController>());

                // 최종 정렬
                FreecellClassicLayoutManager.Instance.UpdateLayout(slot);

                PlayMoveSfx();
            });

        return tw;
    }
    // -----------------------------------------------------------
    // B. 그룹 이동 — 대표 카드만 Tween → 완료 시 전체 정렬
    // -----------------------------------------------------------
    public Tweener MoveGroupToSlot(
    List<StaticCardController> group,
    Transform slot,
    float duration = -1f)
    {
        if (group == null || group.Count == 0)
            return null;

        duration = ResolveDuration(duration);

        var first = group[0].GetComponent<RectTransform>();

        // ---------------------------
        // 1) 그룹 상대 오프셋 저장
        // ---------------------------
        List<Vector3> offsets = new List<Vector3>(group.Count);
        for (int i = 0; i < group.Count; i++)
            offsets.Add(group[i].transform.position - first.position);

        SlotController sc = PrepareSlotAndGetTarget(slot, group.Count, out Vector3 worldTarget);

        // ---------------------------
        // 2) 대표 카드만 Tween
        // ---------------------------
        Tweener tw = first.DOMove(worldTarget, duration)
            .SetEase(Ease.OutCubic)
            .OnUpdate(() =>
            {
                // ---------------------------
                // 3) OnUpdate에서 그룹 전체 이동
                // ---------------------------
                for (int i = 0; i < group.Count; i++)
                    group[i].transform.position = first.position + offsets[i];
            })
            .OnComplete(() =>
            {
                // ---------------------------
                // 4) Tween 끝난 뒤 슬롯에 등록
                // ---------------------------
                foreach (var card in group)
                {
                    var rt = (RectTransform)card.transform;
                    rt.SetParent(slot, worldPositionStays: false);
                    sc.AddCard(card);
                }

                FreecellClassicLayoutManager.Instance.UpdateLayout(slot);
                PlayMoveSfx();
            });

        return tw;
    }

    public void PlayMoveSfx()
    {
        if (audioSource != null && moveSfx != null)
            audioSource.PlayOneShot(moveSfx);
    }

    private float ResolveDuration(float duration)
    {
        return duration <= 0 ? defaultDuration : duration;
    }

    private SlotController PrepareSlotAndGetTarget(Transform slot, int incomingCount, out Vector3 worldTarget)
    {
        var layoutManager = FreecellClassicLayoutManager.Instance;
        SlotController sc = slot.GetComponent<SlotController>();

        Canvas.ForceUpdateCanvases();
        if (!layoutManager.IsApplied)
            layoutManager.ApplyLayout(0f);

        layoutManager.UpdateLayout(slot);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)slot);

        int targetIndex = sc.Cards.Count;
        int totalCount = targetIndex + Mathf.Max(0, incomingCount);
        Vector2 localPos = GetProjectedLocalPosition(layoutManager, slot, targetIndex, totalCount);
        worldTarget = ((RectTransform)slot).TransformPoint(localPos);

        return sc;
    }

    private Vector2 GetProjectedLocalPosition(
        FreecellClassicLayoutManager layoutManager,
        Transform slot,
        int index,
        int totalCount)
    {
        if (layoutManager == null || slot == null)
            return Vector2.zero;

        if (slot.GetComponent<TableauController>() != null)
        {
            RectTransform slotRT = slot as RectTransform;
            float containerHeight = layoutManager.tableausArea != null
                ? layoutManager.tableausArea.rect.height
                : slotRT.rect.height;

            int projectedCount = Mathf.Max(totalCount, index + 1);
            float gap = CalculateDynamicGap(
                layoutManager.CardHeight,
                layoutManager.defaultGapDivisor,
                layoutManager.gapScaleStep,
                containerHeight,
                projectedCount);

            return new Vector2(0f, -gap * index);
        }

        return Vector2.zero;
    }

    private float CalculateDynamicGap(
        float cardHeight,
        float defaultGapDivisor,
        float gapScaleStep,
        float containerHeight,
        int cardCount)
    {
        if (cardCount <= 1)
            return cardHeight / defaultGapDivisor;

        float scale = 1f;
        while (true)
        {
            float gap = cardHeight / (defaultGapDivisor * scale);
            float totalHeight = cardHeight + gap * (cardCount - 1);

            if (totalHeight <= containerHeight)
                return gap;

            scale += gapScaleStep;
        }
    }
}
