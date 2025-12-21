//Assets/Scripts/CardMotionService.cs
using UnityEngine;
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
        if (duration <= 0)
            duration = defaultDuration;

        // 드래그 레이어 가져오기 (없으면 그대로 이동)
        var dragLayer = FreecellClassicLayoutManager.Instance.DragLayer;
        if (dragLayer != null && card.parent != dragLayer)
        {
            // 드래그 레이어로 먼저 이동
            card.SetParent(dragLayer, worldPositionStays: true);
        }

        // 목적지 슬롯의 레이아웃을 먼저 업데이트하여 정확한 위치 계산
        var layoutManager = FreecellClassicLayoutManager.Instance;
        
        // 레이아웃 변경 후 정확한 위치 계산을 위해 캔버스 강제 업데이트
        Canvas.ForceUpdateCanvases();
        
        // 목적지 슬롯의 현재 카드들 정렬 업데이트
        layoutManager.UpdateLayout(slot);

        // Slot 내에서 최종 목적지 계산 (카드 추가 후의 최종 위치)
        // GetLocalPosition 내부에서 레이아웃이 적용되지 않았다면 자동으로 계산됨
        SlotController sc = slot.GetComponent<SlotController>();
        int targetIndex = sc.Cards.Count;
        Vector2 localPos = layoutManager.GetLocalPosition(slot, targetIndex);
        Vector3 worldTarget = ((RectTransform)slot).TransformPoint(localPos);

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

        if (duration <= 0)
            duration = defaultDuration;

        var first = group[0].GetComponent<RectTransform>();
        SlotController sc = slot.GetComponent<SlotController>();

        // ---------------------------
        // 1) 그룹 상대 오프셋 저장
        // ---------------------------
        List<Vector3> offsets = new List<Vector3>(group.Count);
        for (int i = 0; i < group.Count; i++)
            offsets.Add(group[i].transform.position - first.position);

        // ---------------------------
        // 2) 목적지 슬롯의 레이아웃을 먼저 업데이트하여 정확한 위치 계산
        // ---------------------------
        var layoutManager = FreecellClassicLayoutManager.Instance;
        
        // 레이아웃 변경 후 정확한 위치 계산을 위해 캔버스 강제 업데이트
        Canvas.ForceUpdateCanvases();
        
        // 목적지 슬롯의 현재 카드들 정렬 업데이트
        layoutManager.UpdateLayout(slot);

        // ---------------------------
        // 3) 대표 카드 목표 월드 위치 계산 (카드 추가 후의 최종 위치)
        // ---------------------------
        // GetLocalPosition 내부에서 레이아웃이 적용되지 않았다면 자동으로 계산됨
        int targetIndex = sc.Cards.Count;
        Vector2 localPos = layoutManager.GetLocalPosition(slot, targetIndex);
        Vector3 worldTarget = ((RectTransform)slot).TransformPoint(localPos);

        // ---------------------------
        // 4) 대표 카드만 Tween
        // ---------------------------
        Tweener tw = first.DOMove(worldTarget, duration)
            .SetEase(Ease.OutCubic)
            .OnUpdate(() =>
            {
                // ---------------------------
                // 5) OnUpdate에서 그룹 전체 이동
                // ---------------------------
                for (int i = 0; i < group.Count; i++)
                    group[i].transform.position = first.position + offsets[i];
            })
            .OnComplete(() =>
            {
                // ---------------------------
                // 6) Tween 끝난 뒤 슬롯에 등록
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
}