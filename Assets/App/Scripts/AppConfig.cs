using UnityEngine;

[CreateAssetMenu(menuName = "App/AppConfig", fileName = "AppConfig")]
public class AppConfig : ScriptableObject
{
    [Header("Remote Catalog URL")]
    public string remoteCatalogUrl = ""; // 예: https://cdn.example.com/catalog.json

    [Header("Server URL")]
    public string serverUrl = ""; //테스트 : http://hyemini.com/orsgs/cgm

    [Header("Localization Default")]
    public SystemLanguage defaultLanguage = SystemLanguage.Unknown;

    [Header("Version Info")]
    public string buildVersion = "1.0.10";
}