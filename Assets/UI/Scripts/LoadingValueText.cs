using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingValueText : MonoBehaviour
{
    [SerializeField] private Slider slider;
    private TextMeshProUGUI _text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    
    private const string TEXT_SUFFIX = " %";
    public void OnChangedSliderValue()
    {
        _text.text = ((int)slider.value).ToString() + TEXT_SUFFIX;
    }
}
