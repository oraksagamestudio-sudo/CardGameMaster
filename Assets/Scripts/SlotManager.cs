//Assets/Scripts/Solitaire/FreecellClassic/SlotManager.cs
using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 슬롯의 모델초기화와 UI 레이아웃을 관리하기 위한 컨트롤러 매니지먼트 객체의 스크립트
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


}
