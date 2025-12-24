using UnityEngine;
using UnityEngine.UI;

public abstract class AbstractPopupView : MonoBehaviour
{

    public virtual void OnOk() {
        gameObject.SetActive(false);
    }
    public virtual void OnCancel() {
        gameObject.SetActive(false);
    }




}
