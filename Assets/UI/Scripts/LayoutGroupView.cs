//Assets/Scripts/LayoutGroupView.cs
using UnityEngine;
using UnityEngine.UI;
// #if UNITY_EDITOR
// [ExecuteAlways]
// #endif
public abstract class LayoutGroupView : MonoBehaviour
{
    void Awake()
    {
        // UILayoutBroadcaster.Register(this);
    }

    // public void OnCellSizeChanged()
    // {
    //     UpdateCellSize();
    // }

    // public abstract void UpdateCellSize();
}
