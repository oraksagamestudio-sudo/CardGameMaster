using UnityEngine;

public class ColumnPresenterManager : MonoBehaviour
{
    // public static ColumnPresenterManager Instance { get; private set; }

    // public TableauController[] columns;

    // private void Awake()
    // {
    //     Instance = this;
    // }

    // public bool HandleDrop(CardPresenter card, Vector2 screenPos)
    // {
    //     TableauController target = DetectDropTarget(screenPos);
    //     if (target == null)
    //     {
    //         // 어디에도 떨어지지 않음 → 원래 자리 복귀
    //         card.transform.SetParent(card.transform, false);
    //         return false;
    //     }

    //     // 룰 검사는 나중 단계에서 추가
    //     // if (!target.CanDrop(card.model)) { ... }

    //     ApplyMove(card, target);
    //     return true;
    // }

    // private TableauController DetectDropTarget(Vector2 screenPos)
    // {
    //     foreach (var c in columns)
    //     {
    //         if (c == null) continue;

    //         RectTransform rect = c.Rect;
    //         if (RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos))
    //             return c;
    //     }

    //     return null;
    // }

    // private void ApplyMove(CardPresenter card, TableauController target)
    // {
    //     card.transform.SetParent(target.Rect, false);
    //     target.AddCard(card);
    // }
}