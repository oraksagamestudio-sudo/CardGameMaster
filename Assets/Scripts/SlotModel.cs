using UnityEngine;

[System.Serializable]
public class SlotModel
{
    public SlotType Type;
    public int Index;

    public SlotModel(SlotType type, int index)
    {
        Type = type;
        Index = index;
    }

    public override string ToString()
        => $"{Type}[{Index}]";
}