// Assets/Resources/Scripts/CardOptions.cs
public enum CardSuit
{
    Spade = 0, Heart = 1, Club = 2, Diamond = 3
}

public enum CardRank
{
    Ace=1, Two=2, Three=3, Four=4, Five=5, Six=6, Seven=7, Eight=8, Nine=9, Ten=10, Jack=11, Queen=12, King=13
}

public enum ColumnType
{
    Freecell1, Freecell2, Freecell3, Freecell4,
    TempFreecell,
    FoundationSpade, FoundationHeart, FoundationClub, FoundationDiamond,
    Tableau1, Tableua2, Tableau3, Tableau4, Tableau5, Tableau6, Tableau7, Tableau8
}

public enum ShuffleKind
{
    Dotnet,
    Xor
}

