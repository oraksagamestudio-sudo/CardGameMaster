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

        // Slot 내에서 최종 목적지 계산
        SlotController sc = slot.GetComponent<SlotController>();
        int targetIndex = sc.Cards.Count;
        Vector2 localPos = SlotManager.Instance.GetLocalPosition(slot, targetIndex);
        Vector3 worldTarget = ((RectTransform)slot).TransformPoint(localPos);

        // ★ Tween: 월드 좌표로 이동
        Tweener tw = card.DOMove(worldTarget, duration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                // 부모 변경
                card.SetParent(slot, worldPositionStays: false);

                // SlotController 등록
                sc.AddCard(card.GetComponent<StaticCardController>());

                // 최종 정렬
                SlotManager.Instance.UpdateLayout(slot);

                PlayMoveSfx();
            });

        return tw;
    }// -----------------------------------------------------------
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
        // 2) 대표 카드 목표 월드 위치 계산
        // ---------------------------
        int targetIndex = sc.Cards.Count;
        Vector2 localPos = SlotManager.Instance.GetLocalPosition(slot, targetIndex);
        Vector3 worldTarget = ((RectTransform)slot).TransformPoint(localPos);

        // ---------------------------
        // 3) 대표 카드만 Tween
        // ---------------------------
        Tweener tw = first.DOMove(worldTarget, duration)
            .SetEase(Ease.OutCubic)
            .OnUpdate(() =>
            {
                // ---------------------------
                // 4) OnUpdate에서 그룹 전체 이동
                // ---------------------------
                for (int i = 0; i < group.Count; i++)
                    group[i].transform.position = first.position + offsets[i];
            })
            .OnComplete(() =>
            {
                // ---------------------------
                // 5) Tween 끝난 뒤 슬롯에 등록
                // ---------------------------
                foreach (var card in group)
                {
                    var rt = (RectTransform)card.transform;
                    rt.SetParent(slot, worldPositionStays: false);
                    sc.AddCard(card);
                }

                SlotManager.Instance.UpdateLayout(slot);
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