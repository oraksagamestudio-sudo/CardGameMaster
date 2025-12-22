using UnityEngine;
using UnityEngine.UI;

public abstract class AbstractPopupView : MonoBehaviour, IPopupView
{
    [SerializeField] private RectTransform buttonsParent;
    public void SetButtonLayout(GameObject butonLayoutPrefab)
    {
        var buttonsLayout = Instantiate(butonLayoutPrefab, buttonsParent);
        buttonsLayout.SetActive(true);
    }

    public virtual void OnOk() {
        // TODO: this Popup off
    }
    public virtual void OnCancel() {
        // TODO: this Popup off
    }
    public virtual void OnErrorWithForceExit() {

    }

    public abstract void OnNextForGameVictory();
    public abstract void OnRetryForGameVictory();




}
