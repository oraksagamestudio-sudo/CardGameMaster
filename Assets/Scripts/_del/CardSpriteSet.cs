// Assets/Resources/Scripts/CardSpriteSet.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardSpriteSet", menuName = "Cards/SpriteSet")]
public class CardSpriteSet : ScriptableObject
{
    public Sprite backside;

    [System.Serializable]
    public struct CardEntry
    {
        public CardSuit suit;
        public CardRank rank;
        public Sprite sprite;
    }

    public List<CardEntry> entries = new List<CardEntry>();

    private Dictionary<(CardSuit, CardRank), Sprite> _map;

    public Sprite GetSprite(CardSuit suit, CardRank rank)
    {
        if (_map == null)
        {
            _map = new Dictionary<(CardSuit, CardRank), Sprite>();
            foreach (var e in entries)
                _map[(e.suit, e.rank)] = e.sprite;
        }

        return _map[(suit, rank)];
    }
    


}