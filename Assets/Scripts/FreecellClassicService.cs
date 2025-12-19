using UnityEngine;
using UnityEngine.SceneManagement;

public class FreecellClassicService : MonoBehaviour
{
    [SerializeField] private TestController controller;
    private int _lastHandledSceneHandle = -1;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        HandleScene(SceneManager.GetActiveScene());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleScene(scene);
    }

    private void HandleScene(Scene scene)
    {
        if (_lastHandledSceneHandle == scene.handle)
            return;

        _lastHandledSceneHandle = scene.handle;
        //var controller = Object.FindAnyObjectByType<TestController>();
        controller.GameStart();
    }
}
