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

    [Header("defaultPopup")] 
    [SerializeField] protected GameObject defaultPopupPrefab;
    [SerializeField] protected TextMeshProUGUI defaultPopupMessageText;
    [SerializeField] protected Button defaultPopupOkButton;
    [SerializeField] protected Button defaultPopupCancelButton;
    
    [Header("Specific Popups")]
    [SerializeField] protected GameObject pausePopupPrefab;
    [SerializeField] protected GameObject victoryPopupPrefab;
    [SerializeField] protected GameObject settingPopupPrefab;
    [SerializeField] private Transform popupArea;

    public PopupResult popupResult = PopupResult.None;

    protected virtual void Awake() {
        Instance = this;
    }

    public virtual void Show(
        PopupType popupType, 
        string message, 
        string buttonText = null, 
        bool isModal = false, 
        UnityAction onOk = null, 
        bool showCloseButton = true) 
    {
        if(message == null) 
        {
            Debug.LogError("PopupManager.Show: message is null");
            return;
        }
        GameObject backgroundForModalPopup = null;
        if (isModal) 
        {
            backgroundForModalPopup = new GameObject("PopupBackground");
            
            backgroundForModalPopup.transform.SetParent(popupArea, false);
            backgroundForModalPopup.transform.localPosition = Vector3.zero;
            var backgroundRect = backgroundForModalPopup.AddComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
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
        // Set message
        var messageText = popup.transform.Find("MessageArea/Text (TMP)")?.GetComponent<TextMeshProUGUI>();
        if (messageText != null) {
            messageText.text = message;
        }

        // Set button
        var okButton = popup.transform.Find("ButtonArea/Button")?.GetComponent<Button>();
        if (okButton == null)
        {
            okButton = popup.GetComponentInChildren<Button>(true);
            if (okButton == null)
            {
                Debug.LogError("PopupManager.Show: No Button found in popup.");
                Destroy(popup);
                return;
            }
            Debug.LogWarning("Popup prefab has no Button under ButtonArea/Button. Using first Button found in children.");
        }

        if (buttonText != null)
        {
            var okButtonText = okButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (okButtonText != null)
            {
                okButtonText.text = buttonText;
            }
        }

        if (onOk != null)
        {
            okButton.onClick.AddListener(() =>
            {
                popupResult = PopupResult.Ok;
                onOk.Invoke();
            });
        }
        else
        {
            okButton.onClick.AddListener(() =>
            {
                popupResult = PopupResult.Ok;
                Destroy(popup);
                if(backgroundForModalPopup != null) 
                {
                    Destroy(backgroundForModalPopup);
                }
            });
        }
        popup.SetActive(true);
        
    }


}
