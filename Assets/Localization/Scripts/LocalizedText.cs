using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    public LocalizedString key; // 인스펙터에서 테이블/키 지정
    private LocalizeStringEvent _evt;

    // private void Awake()
    // {
    //     _evt = gameObject.AddComponent<LocalizeStringEvent>();
    //     _evt.StringReference = key;
    //     _evt.OnUpdateString.AddListener(UpdateLabel);
    // }

    private async void OnEnable()
    {
        // Localization 시스템 준비 완료 후 등록
        await UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation.Task;

        if (_evt == null)
            _evt = gameObject.AddComponent<LocalizeStringEvent>();

        _evt.StringReference = key;
        _evt.OnUpdateString.RemoveListener(UpdateLabel);
        _evt.OnUpdateString.AddListener(UpdateLabel);

        // ✅ 초기 언어로 즉시 업데이트
        _evt.RefreshString();
    }

    private void UpdateLabel(string value)
    {
        GetComponent<TextMeshProUGUI>().text = value;
    }
}