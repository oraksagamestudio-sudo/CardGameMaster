// StageButton.cs
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;


public class StageButtonController : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI labelUGUI;

    private int stageNumber;
    private Action<int> onClick;

    // 외부에서 호출: 스테이지 번호와 클릭 콜백 주입
    public async Task Setup(int stage, Action<int> onClicked)
    {
        stageNumber = stage;
        onClick = onClicked;

        LocalizedString stageLString = new LocalizedString("default", "lobby_stage");
        stageLString.StringChanged += OnLocalizedTextChanged;
        
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(stageNumber));
        }
    }

    // 필요시 외부에서 라벨만 갱신
    public void SetLabel(string text)
    {
        if (labelUGUI != null) labelUGUI.text = text;
    }

    public void OnLocalizedTextChanged(string value)
    {

        var currentLang = LocalizationSettings.SelectedLocale;
        string langCode = currentLang.Identifier.Code;


        
        string text;
        if (langCode.ToLower()=="ko")
            text = $"{stageNumber} {value}";
        else
            text = $"{value} {stageNumber}";

        if (labelUGUI != null) labelUGUI.text = text;
    }
}