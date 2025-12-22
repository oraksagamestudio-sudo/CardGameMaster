using System.Collections.Generic;
using UnityEngine;

public enum PopupType {
    Normal, Ok, OkCancel, GameVictory, Pause, Setting, Error, ErrorWithForceExit
}

public enum PopupResult {
    None, Ok, Next, Retry
}
public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; protected set; }

    [Header("prefabs")]
    [SerializeField] protected GameObject popupPrefab;
    [SerializeField] protected Dictionary<PopupType, GameObject> buttonLayoutPrefabs;
    [SerializeField] private Transform rootCanvas;

    public PopupResult popupResult = PopupResult.None;

    protected virtual void Awake() {
        Instance = this;
    }

    public virtual void Show(PopupType popupType, string buttonText = null, bool isModal = false) {
        var popup = Instantiate(popupPrefab, rootCanvas);
        var popupView = popup.GetComponent<IPopupView>();

        if(popupType == PopupType.Normal)
            popupType = PopupType.Ok;

        GameObject btnLayoutPrefab = buttonLayoutPrefabs[popupType];

        popupView.SetButtonLayout(btnLayoutPrefab);
        popup.SetActive(true);
        
    }


}