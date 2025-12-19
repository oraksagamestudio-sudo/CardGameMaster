// Assets/Resources/Scripts/CardPresenter.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class CardPresenter : MonoBehaviour,
    IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardModel model;
    public ICardView view;
    private Transform originalParent;


    public void Bind(CardModel m)
    {
        model = m;
        UpdateView();
    }

    private void UpdateView()
    {
        view.SetFace(model.IsFaceUp);
        // 기타 Rank, Suit 설정
    }

    public void OnPointerDown(PointerEventData e)
    {
        // 클릭 반응
    }
    public void OnBeginDrag(PointerEventData e)
    {
        originalParent = transform.parent;
        transform.SetParent(DragLayerService.Instance.transform, true);
    }

    public void OnDrag(PointerEventData e)
    {
        RectTransform rt = (RectTransform)transform;
        rt.anchoredPosition = DragLayerService.Instance.ScreenToLocal(e.position);
    }

    public void OnEndDrag(PointerEventData e)
    {
        // bool success = ColumnPresenterManager.Instance.HandleDrop(this, e.position);
        
        // if (!success)
        // {
        //     // 실패 → 원래 자리로 복귀
        //     transform.SetParent(originalParent, true);
        //     ((RectTransform)transform).anchoredPosition = Vector2.zero;
        // }
        // else
        // {
        //     // 성공 → ColumnPresenterManager에서 새 parent로 붙여줘야 함
        // }
    }
}