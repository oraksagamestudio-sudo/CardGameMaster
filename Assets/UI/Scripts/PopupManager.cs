using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] protected GameObject defaultPopupPrefab;
    [SerializeField] protected GameObject pausePopupPrefab;
    [SerializeField] protected GameObject victoryPopupPrefab;
    [SerializeField] protected GameObject settingPopupPrefab;
    [SerializeField] private Transform popupArea;

    public PopupResult popupResult = PopupResult.None;

    protected virtual void Awake() {
        Instance = this;
    }

    public virtual void Show(PopupType popupType, string buttonText = null, bool isModal = false, Action<PopupResult> onPopupClosed = null) 
    {
        if(isModal) 
        {
            var backgroundForModalPopup = new GameObject("PopupBackground");
            
            backgroundForModalPopup.transform.SetParent(popupArea, false);
            backgroundForModalPopup.transform.localPosition = Vector3.zero;
            backgroundForModalPopup.AddComponent<RectTransform>().sizeDelta = Vector2.zero;
            var bgImg = backgroundForModalPopup.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.5f);
            bgImg.raycastTarget = true;
            bgImg.maskable = true;
            backgroundForModalPopup.AddComponent<CanvasRenderer>();
            var cg = backgroundForModalPopup.AddComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.alpha = 1f;
        
        }



        GameObject popupPrefab = defaultPopupPrefab;

        var popup = Instantiate(popupPrefab, popupArea);
        

        popup.SetActive(true);
        
    }


}