public interface IGameState<TMove>
{
    int MoveCount { get; }
    bool IsVictory { get; }
    bool IsStalemate { get; }
    System.Collections.Generic.IEnumerable<TMove> GetLegalMoves();
    IGameState<TMove> Apply(TMove move);
}