using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(LogoView))]
public class LogoController : MonoBehaviour
{
    private LogoView _view;

    void Awake()
    {
        _view = GetComponent<LogoView>();
    }

    void OnEnable()
    {
        _view.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        _view.gameObject.SetActive(false);
    }

    public IEnumerator ShowLogoAsync(
        float displayTime, 
        bool useBackground = true, 
        bool isIncludeFadeTime = true,
        float fadeInTime = 0.5f,
        float fadeOutTime = 0.5f)
    {
        if(displayTime <= 0)
            yield break;
        
        if(useBackground)
            _view.ShowBackground();
        else
            _view.HideBackground();

        
        displayTime = Mathf.Max(0, displayTime - (isIncludeFadeTime ? (fadeInTime + fadeOutTime) : 0));
        yield return StartCoroutine(_view.ShowLogoRoutine(
            displayTime, 
            fadeInTime));
        yield return new WaitForSeconds(displayTime);

        yield return StartCoroutine(_view.HideLogoRoutine(
            fadeOutTime));
    }
    public void HideLogo()
    {
        _view.HideLogoImmediately();
    }




}
