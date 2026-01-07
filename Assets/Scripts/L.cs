using UnityEngine;
using UnityEngine.Localization.Settings;

public static class L
{
    public static string S(string tableName, string key)
        => LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
}
