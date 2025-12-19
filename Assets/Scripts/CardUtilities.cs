// Assets/Scripts/CardUtilities.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class CardShuffleUtility
{

    private static uint _seed = 1;

    // MSVC rand() 재현
    private static int Rand()
    {
        _seed = (_seed * 1103515245 + 12345) & 0x7fffffff;
        return (int)(_seed >> 16);
    }

    public static int[] NewDeck(uint seed)
    {
        _seed = seed;

        int[] deck = new int[52];
        int[] result = new int[52];

        // 0~51 고유 값 초기화
        for (int i = 0; i < 52; i++)
            deck[i] = i;

        int wLeft = 52;

        for (int i = 0; i < 52; i++)
        {
            int j = Rand() % wLeft;  // 뽑을 위치
            result[i] = deck[j];     // 선택한 카드 저장

            // deck[j] 자리를 deck[wLeft-1]로 채워넣고
            deck[j] = deck[--wLeft];
        }

        return result;
    }
}

public static class CardIndexUtility
{
    // suit * 13 + (rank - 1)
    public static int ToIndex(CardSuit suit, CardRank rank)
    {
        return ((int)suit * 13) + ((int)rank - 1);
    }

    public static void FromIndex(int index, out CardSuit suit, out CardRank rank)
    {
        suit = (CardSuit)(index / 13);
        rank = (CardRank)((index % 13) + 1);
    }
}
