using UnityEngine;

public class LoadingPanelController : MonoBehaviour
{
    private LoadingPanelModel _model;
    private LoadingPanelView _view;

    public void Initialize(LoadingPanelModel model)
    {
        _model = model;
        _view = GetComponentInChildren<LoadingPanelView>();

        _view.ApplyInitialState(_model);

        _model.OnProgressTextChanged += _view.UpdateProgress;
    }

    public void SetProgress(float value, string text = null)
    {
        _model.SetProgress(value, text);
    }

    public void Complete()
    {
        if (_model.RequireTouchToContinue)
        {
            // TouchToContinue UI 활성화
        }
        else
        {
            _model.OnCompleted?.Invoke();
            Destroy(gameObject);
        }
    }

    public void Fail()
    {
        _model.OnFailed?.Invoke();
        Destroy(gameObject);
    }
}