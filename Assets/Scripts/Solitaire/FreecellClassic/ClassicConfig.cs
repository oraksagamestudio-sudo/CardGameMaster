namespace Solitaire.FreecellClassic
{
    public sealed class ClassicConfig
    {
        public int Cells { get; }
        public int Foundations { get; }
        public int Tableaus { get; }
        public bool AllowSequenceMoves { get; }

        public ClassicConfig(
            int cells = 4,
            int foundations = 4,
            int tableaus = 8,
            bool allowSequenceMoves = true
        )
        {
            Cells = cells;
            Foundations = foundations;
            Tableaus = tableaus;
            AllowSequenceMoves = allowSequenceMoves;
        }

        public static ClassicConfig Default => new();
    }
}