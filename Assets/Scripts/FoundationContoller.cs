// Assets/Scripts/FoundationController.cs
using System.Collections.Generic;
using UnityEngine;

public class FoundationController : SlotController
{
    [SerializeField] private CardSuit suit;

    void Awake()
    {
        // suit → 엔진 Index 매핑
        int index = suit switch
        {
            CardSuit.Spade => 0,
            CardSuit.Heart => 1,
            CardSuit.Club => 2,
            CardSuit.Diamond => 3,
            _ => 0
        };

        Init(SlotType.Foundation, index);
    }
}
