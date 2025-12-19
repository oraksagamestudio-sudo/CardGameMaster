// Assets/Scripts/DragLayerService.cs
using UnityEngine;

public class DragLayerManager : MonoBehaviour
{
    public static DragLayerManager Instance { get; private set; }
    public RectTransform Rect => (RectTransform)transform;

    private void Awake()
    {
        Instance = this;
    }

    public Vector2 ScreenToLocal(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            Rect,
            screenPos,
            null,
            out Vector2 localPos
        );
        return localPos;
    }
}