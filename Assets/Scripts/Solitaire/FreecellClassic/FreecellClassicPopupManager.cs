using UnityEngine;

public class FreecellClassicPopupManager : PopupManager
{
    public static new FreecellClassicPopupManager Instance { get; private set; }

    protected override void Awake()
    {
        Instance = this;
    }
}
