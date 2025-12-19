using System.Collections.Generic;
using UnityEngine;

public static class CardSpriteUtility
{
    private static readonly Dictionary<string, Sprite> _cache = new();

    // private static GameObject _cardPrefab;
    //private static RectTransform _refRect;
    // private static Vector2 _cardSize;
    public static void Init(GameObject cardPrefab)
    {
        // var rt = cardPrefab.GetComponent<RectTransform>();
        // _cardSize = rt.rect.size;
        _cache.Clear();
    }
    public static string Theme { get; set; } = "default";
    // 다른 스킨 적용 시 Theme만 변경하면 끝.
    //public static RectTransform ReferenceCardRect => _refRect;
    //public static Vector2 CardSize => _cardSize;
    public static Sprite GetSprite(CardSuit suit, CardRank rank)
    {
        string suitCode = suit switch
        {
            CardSuit.Spade => "S",
            CardSuit.Heart => "H",
            CardSuit.Club => "C",
            CardSuit.Diamond => "D",
            _ => "?"
        };

        string rankCode = rank switch
        {
            CardRank.Ace => "A",
            CardRank.Two => "2",
            CardRank.Three => "3",
            CardRank.Four => "4",
            CardRank.Five => "5",
            CardRank.Six => "6",
            CardRank.Seven => "7",
            CardRank.Eight => "8",
            CardRank.Nine => "9",
            CardRank.Ten => "T",
            CardRank.Jack => "J",
            CardRank.Queen => "Q",
            CardRank.King => "K",
            _ => "?"
        };

        string path = $"Cards/{Theme}/card_{suitCode}{rankCode}";

        if (_cache.TryGetValue(path, out var sp))
            return sp;

        sp = Resources.Load<Sprite>(path);
        if (sp == null)
        {
            Debug.LogError($"[CardSpriteProvider] Sprite not found: {path}");
            return null;
        }

        _cache[path] = sp;
        return sp;
    }

    public static Sprite GetBackside()
    {
        string path = $"Cards/{Theme}/card_BG";
        if (_cache.TryGetValue(path, out var sp))
            return sp;

        sp = Resources.Load<Sprite>(path);
        if (sp == null)
        {
            Debug.LogError($"[CardSpriteProvider] Sprite not found: {path}");
            return null;
        }

        _cache[path] = sp;
        return sp;
    }
}