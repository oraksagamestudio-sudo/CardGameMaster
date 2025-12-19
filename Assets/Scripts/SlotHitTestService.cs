// Assets/Scripts/SlotHitTestService.cs
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SlotHitTestService : MonoBehaviour
{
    public static SlotHitTestService Instance;

    // 드롭 가능한 슬롯 목록
    //[SerializeField] private List<RectTransform> slotTargets = new List<RectTransform>();


    void Awake()
    {
        Instance = this;
    }

    
}