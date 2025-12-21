//Assets/Scripts/StaticCardView.cs
using UnityEngine;
using UnityEngine.UI;

public class StaticCardView : MonoBehaviour, ICardView
{
    private Image _img;

    private CardSuit _suit;
    private CardRank _rank;
    private bool _faceUp = true;

    private void Awake()
    {
        _img = GetComponent<Image>();
        // UILayoutBroadcaster.Register(this);
    }
    private void Start()
    {
        // UILayoutBroadcaster.Broadcast();
    }

    // ---------------------------
    // Unified data setter
    // ---------------------------
    public void SetCardData(CardModel model)
    {
        SetCardData(model.Suit, model.Rank, model.IsFaceUp);
    }

    public void SetCardData(CardSuit suit, CardRank rank, bool faceUp)
    {
        _suit = suit;
        _rank = rank;
        _faceUp = faceUp;

        ApplySprite();
    }

    // ---------------------------
    // Face toggle
    // ---------------------------
    public void SetFace(bool faceUp)
    {
        _faceUp = faceUp;
        ApplySprite();
    }

    // ---------------------------
    // Rank / Suit 변경
    // ---------------------------
    public void SetRank(CardRank rank)
    {
        _rank = rank;
        if (_faceUp)
            ApplySprite();
    }

    public void SetSuit(CardSuit suit)
    {
        _suit = suit;
        if (_faceUp)
            ApplySprite();
    }

    // ---------------------------
    // Apply sprite (centralized logic)
    // ---------------------------
    private void ApplySprite()
    {
        if (_faceUp)
            _img.sprite = CardSpriteUtility.GetSprite(_suit, _rank);
        else
            _img.sprite = CardSpriteUtility.GetBackside();
    }

    public void SetDraggingVisual(bool fade)
    {
        var color = _img.color;
        color.a = fade?.5f:1f;
    }

    public void OnCardSizeChanged()
    {
        UpdateCardSize();
    }

    private void UpdateCardSize()
    {
        var rt = GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, FreecellClassicLayoutManager.Instance.CardWidth);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, FreecellClassicLayoutManager.Instance.CardHeight);
    }
}