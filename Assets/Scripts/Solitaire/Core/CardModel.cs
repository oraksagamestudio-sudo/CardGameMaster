//Assets/Scripts/Solitaire/Core/CardModel.cs
using System;

public class CardModel
{
    public CardSuit Suit;
    public CardRank Rank;
    public bool IsFaceUp;
    public ColumnType CurrentColumn;
    public int ColumnIndex;

    private bool _hasValue = false;
    public bool HasValue => _hasValue;

    // -----------------------------
    // 공통 매핑 테이블
    // -----------------------------
    // private static readonly char[] SuitChars = { 'S', 'H', 'C', 'D' };
    // private static readonly char[] RankChars =
    //     { 'A','2','3','4','5','6','7','8','9','T','J','Q','K' };

    // 역방향 매핑: 문자 → Suit/Rank 인덱스
    private static readonly string SuitMap = "SHCD";
    private static readonly string RankMap = "A23456789TJQK";

    // -----------------------------
    // 생성자: Suit/Rank 직접 설정
    // -----------------------------
    public CardModel(CardSuit suit, CardRank rank, bool isFaceUp = false)
    {
        Suit = suit;
        Rank = rank;
        IsFaceUp = isFaceUp;
        _hasValue = true;
    }

    // -----------------------------
    // 생성자: "SQ", "H7" 등 두 글자 문자열
    // -----------------------------
    public CardModel(string symbol, bool isFaceUp = false)
    {
        if (symbol == null || symbol.Length != 2)
            throw new IlegalSymbolException();

        symbol = symbol.ToUpper();
        int suitIdx = SuitMap.IndexOf(symbol[0]);
        int rankIdx = RankMap.IndexOf(symbol[1]);

        if (suitIdx < 0 || rankIdx < 0)
            throw new IlegalSymbolException();

        Suit = (CardSuit)suitIdx;
        Rank = (CardRank)(rankIdx + 1);
        IsFaceUp = isFaceUp;
        _hasValue = true;
    }

    // -----------------------------
    // 문자열 변환
    // -----------------------------
    // public override string ToString() =>
    //     $"{Rank} of {Suit}{(IsFaceUp ? "" : " (down)")}";

    public override string ToString()
    {
        var s = SuitMap.ToCharArray();
        var r = RankMap.ToCharArray();
        return $"{s[(int)Suit]}{r[((int)Rank) - 1]}";
    }

    // -----------------------------
    // SuitIndex (전역에서 많이 쓰는 함수)
    // -----------------------------
    public static int SuitIndex(CardSuit s) => (int)s;
}

public class IlegalSymbolException : Exception { }