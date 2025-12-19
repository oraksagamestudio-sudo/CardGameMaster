//Assets/Scripts/SlotController.cs
using System.Collections.Generic;
using UnityEngine;

public class SlotController : MonoBehaviour
{
    public SlotModel Model { get; private set; }
    private readonly List<StaticCardController> _cards = new();

    public IReadOnlyList<StaticCardController> Cards => _cards;

    public void Clear()
    {
        _cards.Clear();
    }

    public virtual void Init(SlotType type, int index)
    {
        Model = new SlotModel(type, index);
    }

    public void AddCard(StaticCardController card)
    {
        _cards.Add(card);
        card.transform.SetParent(transform, false); 
    }

    public void AddGroup(List<StaticCardController> group)
    {
        foreach (var c in group)
            AddCard(c);
    }

    public int GetCardIndex(StaticCardController card)
        => _cards.IndexOf(card);

    public void RemoveFrom(int index)
        => _cards.RemoveAt(index);

    public void InsertAt(int index, StaticCardController card)
    {
        _cards.Insert(index, card);
        card.transform.SetParent(this.transform);
    }

    public void RemoveCard(StaticCardController card)=> _cards.Remove(card);


    // 태블로에서 index 기준으로 "아래 매달린 카드 그룹"을 반환
    public List<StaticCardController> GetCascade(int startIndex)
    {
        if (startIndex < 0 || startIndex >= _cards.Count)
            return new List<StaticCardController>(); // 또는 예외 처리

        return _cards.GetRange(startIndex, _cards.Count - startIndex);
    }



}
