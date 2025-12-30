using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LogoView : MonoBehaviour
{
    [SerializeField] private GameObject logoBackground;
    [SerializeField] private GameObject logoImage;

    internal void ShowBackground()
    {
        logoBackground.SetActive(true);
    }
    internal void HideBackground()
    {
        logoBackground.SetActive(false);
    }

    internal IEnumerator ShowLogoRoutine(float displayTime, float fadeInTime)
    {
        
        var image = logoImage.GetComponent<Image>();
        var newColor = new Color(image.color.r, image.color.g, image.color.b, 0f);
        image.color = newColor;
        logoImage.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            newColor.a = Mathf.Clamp01(elapsed / fadeInTime);
            image.color = newColor;
            yield return null;
        }
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
    }

    internal IEnumerator HideLogoRoutine(float fadeOutTime)
    {
        var image = logoImage.GetComponent<Image>();
        var newColor = new Color(image.color.r, image.color.g, image.color.b, 1f);
        image.color = newColor;

        float elapsed = 0f;
        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            newColor.a = 1f - Mathf.Clamp01(elapsed / fadeOutTime);
            image.color = newColor;
            yield return null;
        }
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
        logoImage.SetActive(false);
    }

    internal void HideLogoImmediately()
    {
        gameObject.SetActive(false);
    }

}
