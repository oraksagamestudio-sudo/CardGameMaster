using UnityEngine;

public abstract class AbstractCardView : MonoBehaviour//, ICardView
{
    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        FreecellUIInitializer.OnCardSizeChanged += ApplySize;
    }

    private void OnDestroy()
    {
        FreecellUIInitializer.OnCardSizeChanged -= ApplySize;
    }

    private void ApplySize(Vector2 size)
    {
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }

    public abstract void SetFace(bool faceUp);

    public abstract void SetRank(CardRank rank);

    public abstract void SetSuit(CardSuit suit);

    public abstract void SetCardData(CardSuit suit, CardRank rank);
}