using UnityEngine;
using UnityEngine.EventSystems;

public class DummyCardController : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    private RectTransform rect;
    private Canvas canvas;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 필요시 클릭시 카드 앞으로 올리기
        rect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 캔버스 스케일 고려해서 위치 이동
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos);

        rect.anchoredPosition = pos;
    }
}