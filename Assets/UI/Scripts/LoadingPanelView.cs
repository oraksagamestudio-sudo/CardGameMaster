using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanelView : MonoBehaviour
{
    [Header("Groups")]
    public GameObject backgroundGroup;
    public GameObject spinner;
    public GameObject tooltipText;
    public GameObject progressText;
    public GameObject percentText;
    public UnityEngine.UI.Slider progressBar;

    [Header("Background Images")]
    public List<Image> backgroundImages;

    public void ApplyInitialState(LoadingPanelModel model)
    {
        if (backgroundGroup) backgroundGroup.SetActive(model.UseBackground);
        if (spinner) spinner.SetActive(model.ShowSpinner);
        if (tooltipText) tooltipText.SetActive(model.ShowTooltip);
        if (progressText) progressText.SetActive(model.ShowProgressText);
        if (percentText) percentText.SetActive(model.ShowPercentText);
    }

    public void UpdateProgress(LoadingProgress progress)
    {
        if (progressBar)
            progressBar.value = progress.Progress;

        if (progressText)
            progressText.GetComponent<TMPro.TextMeshProUGUI>().text = progress.ProgressText;

        if (percentText)
            percentText.GetComponent<TMPro.TextMeshProUGUI>().text =
                $"{Mathf.RoundToInt(progress.Progress * 100f)}%";
    }
}