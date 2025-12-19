// Assets/Resources/Scripts/DynamicCardView.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Collections.Generic;

public class DynamicCardView : MonoBehaviour//, ICardView
{
    [Header("References")]
    public Image background;
    public Image artworkFront;
    public TMP_Text rankText;
    public Image rankSuit;
    public Image suitIcon;

    public RectTransform Rect => (RectTransform)transform;

    public void SetFace(bool faceUp)
    {
        artworkFront.gameObject.SetActive(faceUp);
        rankText.gameObject.SetActive(faceUp);
        rankSuit.gameObject.SetActive(faceUp);
        suitIcon.gameObject.SetActive(faceUp);
    }

    public void SetCardData(CardSuit suit, CardRank rank)
    {
        SetSuit(suit);
        SetRank(rank);
    }

    private static readonly Dictionary<CardRank, string> RankMap =
        new Dictionary<CardRank, string>
        {
            { CardRank.Ace, "A" },
            { CardRank.Two, "2" },
            { CardRank.Three, "3" },
            { CardRank.Four, "4" },
            { CardRank.Five, "5" },
            { CardRank.Six, "6" },
            { CardRank.Seven, "7" },
            { CardRank.Eight, "8" },
            { CardRank.Nine, "9" },
            { CardRank.Ten, "10" },
            { CardRank.Jack, "J" },
            { CardRank.Queen, "Q" },
            { CardRank.King, "K" },
        };

    public void SetRank(CardRank rank)
    {
        SetRank(RankMap[rank]);
    }

    private static readonly Dictionary<Color, string> CardColorMap =
        new Dictionary<Color, string>
        {
            { Color.black,  "#282828" },
            { Color.red,    "#c53030" }
        };
    private static readonly Dictionary<CardSuit, (string spritePath, string colorHex)> SuitMap =
        new Dictionary<CardSuit, (string, string)>
        {
            { CardSuit.Spade,   ("Cards/Suit_Spade",  CardColorMap[Color.black]) },
            { CardSuit.Heart,   ("Cards/Suit_Heart",  CardColorMap[Color.red]) },
            { CardSuit.Club,    ("Cards/Suit_Club",   CardColorMap[Color.black]) },
            { CardSuit.Diamond, ("Cards/Suit_Diamond",CardColorMap[Color.red]) },
        };

    public void SetSuit(CardSuit suit)
    {
        var data = SuitMap[suit];

        UnityEngine.Color color;
        UnityEngine.ColorUtility.TryParseHtmlString(data.colorHex, out color);

        SetSuit(Resources.Load<Sprite>(data.spritePath), color);
    }

    public void SetRank(string rank)
    {
        rankText.text = rank;
    }

    public void SetSuit(Sprite iconSprite, Color color)
    {
        suitIcon.sprite = iconSprite;
        rankSuit.sprite = iconSprite;
        rankText.color = color;

    }


}