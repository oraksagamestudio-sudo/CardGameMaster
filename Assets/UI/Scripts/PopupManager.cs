using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

    public virtual void Show(PopupType popupType, string buttonText = null, bool isModal = false, UnityAction onOk = null) 
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

        var okButton = popup.transform.Find("ButtonArea/Button")?.GetComponent<Button>();
        if (okButton == null) {
            okButton = popup.GetComponentInChildren<Button>(true);
            if(buttonText != null)
                okButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
            if (onOk != null) {
                Debug.LogWarning("Popup prefab has no Button under ButtonArea/Button. Using first Button found in children.");
                popupResult = PopupResult.Ok;
                okButton.onClick.AddListener(onOk);
            }
            else
            {
                okButton.onClick.AddListener(() =>
                {
                    popupResult = PopupResult.Ok;
                    Destroy(popup);
                });
            }
            
        }
        popup.SetActive(true);
        
    }


}
