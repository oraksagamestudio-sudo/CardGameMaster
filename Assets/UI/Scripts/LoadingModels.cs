using System;
using UnityEngine;
using UnityEngine.Events;
public struct LoadingProgress
{
    public float Progress;          // 0~1
    public string ProgressText;
}

public class LoadingPanelModel
{
    // 초기화 파라미터
    public bool UseBackground = false;
    public bool RequireTouchToContinue = false;
    public bool ShowTooltip = false;
    public bool ShowSpinner = true;
    public bool ShowProgressText = true;
    public bool ShowPercentText = true;
    public float StepDelay = 0.01f;

    // 상태
    public float ProgressValue = 0f;
    public string ProgressText = "loading...";
    public string TooltipText = "";

    // 이벤트 
    public event Action<LoadingProgress> OnProgressTextChanged;
    public UnityAction OnCompleted;
    public UnityAction OnFailed;

    public void SetProgress(float value, string text = null)
    {
        ProgressValue = Mathf.Clamp01(value);
        if (text != null)
            ProgressText = text;

        OnProgressTextChanged?.Invoke(
            new LoadingProgress
            {
                Progress = ProgressValue,
                ProgressText = ProgressText
            });
    }
}