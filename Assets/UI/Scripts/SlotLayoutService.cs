//Asset/UI/Scripts/SlotLayoutService.cs
using UnityEngine;
using System.Collections.Generic;

public class SlotLayoutService : MonoBehaviour
{
    public static SlotLayoutService Instance;
    private List<TableauController> allSlots = new();

    void Awake()
    {
        Instance = this;
    }

}