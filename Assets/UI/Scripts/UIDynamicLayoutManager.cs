using System.Collections;
using UnityEngine;

public abstract class UIDynamicLayoutManager : MonoBehaviour
{
    
    [SerializeField] protected float delaySeconds = 0f;
    [SerializeField] protected UIDynamicLayoutManager nextManager;

    public virtual void ApplyLayout(float delaySeconds = 0f)
    {
        if(delaySeconds == 0f)
            CalculateLayoutComponents();
        else 
            StartCoroutine(LayoutRoutine(delaySeconds));
        Canvas.preWillRenderCanvases -= HandleWillRenderCanvases;
        Canvas.preWillRenderCanvases += HandleWillRenderCanvases;
    }

    public virtual void ApplyLayout(int delayFrames = 0)
    {
        if (delayFrames == 0)
            CalculateLayoutComponents();
        else
        {
            StartCoroutine(LayoutRoutine(delaySeconds));
        }

        Canvas.preWillRenderCanvases -= HandleWillRenderCanvases;
        Canvas.preWillRenderCanvases += HandleWillRenderCanvases;
    }


    protected virtual void OnDisable()
    {
        Canvas.preWillRenderCanvases -= HandleWillRenderCanvases;
    }

    public IEnumerator LayoutRoutine(float delaySeconds = 0f)
    {
        yield return new WaitForSeconds(delaySeconds);
        CalculateLayoutComponents();
    }

    public IEnumerator LayoutRoutineWithFrameDelay(int delayFrame = 0)
    {
        for(int i = 0 ; i < delayFrame; i++)
        {
            yield return null;
            //Canvas.ForceUpdateCanvases();
            CalculateLayoutComponents();
        }
    }
    protected abstract void HandleWillRenderCanvases();
    protected abstract void CalculateLayoutComponents();
    public void CallNextLayoutManager(float delaySeconds = 0) => nextManager.ApplyLayout(delaySeconds);

}
