using UnityEngine;

public class FreecellClassicPopupManager : PopupManager
{
    public override void Show(PopupType popupType, string buttonText = null, bool isModal = false)
    {
        base.Show(popupType, buttonText, isModal);
    }
}
