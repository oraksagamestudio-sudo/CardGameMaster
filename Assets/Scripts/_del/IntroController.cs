using UnityEngine;

[RequireComponent(typeof(IntroView))]
public class IntroController : MonoBehaviour
{
    private IntroView _view;

    void Awake()
    {
        _view = GetComponent<IntroView>();
    }
    
    public void SetProgress(float progress, string messageKey)
    {
        //TODO 인트로 로딩 화면에 진행상황 표시
        Debug.Log($"[Intro] Progress: {progress}, MessageKey: {messageKey}");
    }
}
