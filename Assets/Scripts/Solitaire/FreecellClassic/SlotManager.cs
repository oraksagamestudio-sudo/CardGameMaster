//Assets/Scripts/Solitaire/FreecellClassic/SlotManager.cs
using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 슬롯의 모델초기화 등 로직을 위한 컨트롤러 매니지먼트 객체의 스크립트
/// </summary>
public class SlotManager : MonoBehaviour
{

    public static SlotManager Instance;

    [Header("Slots")]
    public List<SlotController> Tableaus = new();
    public List<SlotController> Freecells = new();
    public List<SlotController> Foundations = new();
    public SlotController TempSlot;
    public List<SlotController> AllSlots
    {
        get
        {
            int capacity = Tableaus.Count + Freecells.Count + Foundations.Count + (TempSlot != null ? 1 : 0);
            List<SlotController> result = new(capacity);
            result.AddRange(Tableaus);
            result.AddRange(Freecells);
            result.AddRange(Foundations);
            if (TempSlot != null)
                result.Add(TempSlot);
            return result;
        }
    }

    void Awake()
    {
        Instance = this;

        // Tableau
        for (int i = 0; i < Tableaus.Count; i++)
            Tableaus[i].Init(SlotType.Tableau, i);

        // Freecell
        for (int i = 0; i < Freecells.Count; i++)
            Freecells[i].Init(SlotType.Freecell, i);

        // Foundation
        for (int i = 0; i < Foundations.Count; i++)
            Foundations[i].Init(SlotType.Foundation, i);

        // Temp
        if (TempSlot != null)
            TempSlot.Init(SlotType.Temp, 0);
    }


    public void RegisterSlot(TableauController slot)
    {
        if (!AllSlots.Contains(slot))
            AllSlots.Add(slot);
    }

    public void UpdateAllSlots()
    {
        foreach (var slot in AllSlots)
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

        // 슬롯의 높이
        float slotHeight = slotRT.rect.height;

        // 슬롯 pivot 기준 Top 위치
        float topY = slotHeight * (1f - slotRT.pivot.y);

        // 카드 pivot이 (0.5,1) 이 아닐 수 있으니 카드 높이 구함 
        var uiManager = FreecellClassicLayoutManager.Instance;
        float cardHeight = uiManager.CardHeight;

        // 카드 세로 디폴트 오프셋 (겹쳤을 때 간격)
        float cardGap = cardHeight / uiManager.defaultGapDivisor;
        float offsetY = cardGap;  // 행님이 말한 기본 오프셋

        // "Top에서 시작해서 아래로 쌓는" Y좌표
        float y = topY - (index * offsetY) - (cardHeight * 0.5f);

        return new Vector2(0, y);
    }

    public Vector2 GetLocalPosition(Transform slot, int index)
    {

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
