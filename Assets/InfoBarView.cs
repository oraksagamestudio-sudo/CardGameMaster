using System.Globalization;
using TMPro;
using UnityEngine;
public class InfoBarView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI gameNumberText;

    [SerializeField] private TextMeshProUGUI scoreText;

    public void SetTime(float seconds)
    {
        timeText.text = FormatTime(seconds);
    }
    public void SetGameNumber(uint seed)
    {
        string formattedNumber = seed.ToString("N0", CultureInfo.InvariantCulture);
        gameNumberText.text = $"#{formattedNumber}";
    }

    public void SetScore(int score)
    {
        scoreText.text = score.ToString("N0", CultureInfo.InvariantCulture);
    }

    private static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int totalMinutes = totalSeconds / 60;
        if (totalMinutes < 100)
        {
            int remainingSeconds = totalSeconds - (totalMinutes * 60);
            return $"{totalMinutes:00}:{remainingSeconds:00}";
        }

        int hours = totalMinutes / 60;
        int remainingMinutes = totalMinutes - (hours * 60);
        return $"{hours}:{remainingMinutes:00}";
    }

}
