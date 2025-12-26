using UnityEngine;

public static class GlobalLoadingService
{
    private static LoadingPanelController _current;

    public static void Show(LoadingPanelModel model)
    {
        if (_current != null) return;

        var prefab = Resources.Load<GameObject>("UI/Prefabs/LoadingPanelPrefab");
        var instance = GameObject.Instantiate(prefab);
        _current = instance.GetComponent<LoadingPanelController>();
        _current.Initialize(model);
    }

    public static void SetProgress(float value, string text = null)
    {
        _current?.SetProgress(value, text);
    }

    public static void Complete()
    {
        _current?.Complete();
        _current = null;
    }

    public static void Fail()
    {
        _current?.Fail();
        _current = null;
    }
}