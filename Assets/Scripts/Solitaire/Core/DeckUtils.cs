//Assets/Scripts/Solitaire.Core/DeckUtils.cs
using System.Collections.Generic;

namespace Solitaire.Core
{
    public static class DeckUtils
    {
        public static List<CardModel> CreateStandard52(bool faceUp = true)
        {
            var list = new List<CardModel>(52);
            foreach (CardSuit s in new[] { CardSuit.Spade, CardSuit.Heart, CardSuit.Diamond, CardSuit.Club })
                for (int r = 1; r <= 13; r++)
                    list.Add(new CardModel(s, (CardRank)r, faceUp));
            return list;
        }

        public static void FisherYatesShuffle<T>(IList<T> list, IRng rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                var tmp = list[i]; list[i] = list[j]; list[j] = tmp;
            }
        }
    }
}
