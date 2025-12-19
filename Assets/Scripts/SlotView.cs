//Assets/Scripts/SlotView.cs
using UnityEngine;
// #if UNITY_EDITOR
// [ExecuteAlways]
// #endif
public class SlotView : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //UILayoutBroadcaster.Register(this);

    }

    // public void OnSlotSizeChanged()
    // {
    //     UpdateSlotSize();
        
    // }

    // public void UpdateSlotSize()
    // {
    //     var rt = GetComponent<RectTransform>();
    //     rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CardSizeProvider.CardWidth);
    //     rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CardSizeProvider.CardHeight);
    // }
}
