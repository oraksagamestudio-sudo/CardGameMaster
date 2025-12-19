//Assests/Scripts/Solitaire/FreecellClassic/ClassicState.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Solitaire.Core;

namespace Solitaire.FreecellClassic
{
    /// <summary>
    /// 순수 Freecell Classic 규칙을 담는 immutable State.
    /// Apply()는 항상 새로운 상태를 리턴한다.
    /// </summary>
    public sealed class ClassicState
    {
        public readonly List<CardModel>[] Tableaus;
        public readonly CardModel[] Cells;
        public readonly int[] FoundationTop;
        public readonly int MoveCount;
        public readonly ClassicConfig Config;
        public readonly uint Seed;

        // -------------------------------------------------------
        // Victory
        // -------------------------------------------------------
        public bool IsVictory
        {
            get
            {
                for (int i = 0; i < FoundationTop.Length; i++)
                    if (FoundationTop[i] != 13)
                        return false;
                return true;
            }
        }

        public ClassicState(uint seed, ClassicConfig config,
            List<CardModel>[] tableaus,
            CardModel[] cells,
            int[] foundationTop,
            int moveCount)
        {
            Seed = seed;
            Config = config;
            Tableaus = tableaus;
            Cells = cells;
            FoundationTop = foundationTop;
            MoveCount = moveCount;
        }

        // ------------------------------------------------------
        // New Game
        // ------------------------------------------------------
        public static ClassicState NewGame(uint seed, ClassicConfig cfg, ShuffleKind shuffle)
        {
            var deck = DeckUtils.CreateStandard52(faceUp: true);
            IRng rng = shuffle == ShuffleKind.Dotnet
                ? new DotNetRandom((int)seed)
                : new XorShift32(seed);

            DeckUtils.FisherYatesShuffle(deck, rng);

            var t = new List<CardModel>[cfg.Tableaus];
            for (int i = 0; i < t.Length; i++)
                t[i] = new List<CardModel>();

            for (int i = 0; i < deck.Count; i++)
                t[i % cfg.Tableaus].Add(deck[i]);

            var cells = new CardModel[cfg.Cells];
            var ftop = new int[cfg.Foundations];

            return new ClassicState(seed, cfg, t, cells, ftop, 0);
        }

        // ------------------------------------------------------
        // Clone
        // ------------------------------------------------------
        public ClassicState Clone()
        {
            var t = new List<CardModel>[Tableaus.Length];
            for (int i = 0; i < t.Length; i++)
                t[i] = new List<CardModel>(Tableaus[i]);

            var c = (CardModel[])Cells.Clone();
            var f = (int[])FoundationTop.Clone();

            return new ClassicState(Seed, Config, t, c, f, MoveCount);
        }

        // ------------------------------------------------------
        // GetLegalMoves
        // ------------------------------------------------------
        public IEnumerable<Move> GetLegalMoves()
        {
            // --- 1) Tableau → Foundation / Cell / Tableau ---
            for (int src = 0; src < Tableaus.Length; src++)
            {
                var pile = Tableaus[src];
                if (pile.Count > 0)
                {
                    var top = pile[^1];

                    // Tableau → Foundation
                    if (CanMoveToFoundation(top))
                        yield return new Move(MoveKind.TableauToFoundation, src, SuitIndex(top.Suit));

                    // Tableau → Cell
                    for (int c = 0; c < Cells.Length; c++)
                    {
                        if (Cells[c] == null)
                            yield return new Move(MoveKind.TableauToCell, src, c);
                    }

                    // Tableau → Tableau
                    for (int dst = 0; dst < Tableaus.Length; dst++)
                    {
                        if (src == dst) continue;

                        int movable = MaxMovableSequenceCount(src, dst);
                        if (movable > 0)
                            yield return new Move(MoveKind.TableauToTableau, src, dst, movable);
                    }
                }
            }

            // --- 2) Cell → Foundation / Tableau ---
            for (int c = 0; c < Cells.Length; c++)
            {
                if (Cells[c] != null)
                {
                    var card = Cells[c];

                    // Cell → Foundation
                    if (CanMoveToFoundation(card))
                        yield return new Move(MoveKind.CellToFoundation, c, SuitIndex(card.Suit));

                    // Cell → Tableau
                    for (int dst = 0; dst < Tableaus.Length; dst++)
                        if (CanPlaceOnTableau(Tableaus[dst], card))
                            yield return new Move(MoveKind.CellToTableau, c, dst);
                }
            }

            // --- 3) Foundation → Tableau / Cell ---
            for (int f = 0; f < FoundationTop.Length; f++)
            {
                int topRank = FoundationTop[f];
                if (topRank <= 0)
                    continue;

                var card = new CardModel((CardSuit)f, (CardRank)topRank, true);

                // Foundation → Tableau
                for (int dst = 0; dst < Tableaus.Length; dst++)
                    if (CanPlaceOnTableau(Tableaus[dst], card))
                        yield return new Move(MoveKind.FoundationToTableau, f, dst);

                // Foundation → Cell
                for (int c = 0; c < Cells.Length; c++)
                    if (Cells[c] == null)
                        yield return new Move(MoveKind.FoundationToCell, f, c);
            }
        }

        // ------------------------------------------------------
        // Apply (Immutable State)
        // ------------------------------------------------------
        public ClassicState Apply(Move m)
        {
            var t = CloneTableaus();
            var cells = (CardModel[])Cells.Clone();
            var ftop = (int[])FoundationTop.Clone();
            int moves = MoveCount + 1;

            switch (m.Kind)
            {
                case MoveKind.TableauToCell:
                    {
                        RequireTableau(m.From);
                        RequireCell(m.To);

                        var pile = t[m.From];
                        if (pile.Count == 0)
                            throw new InvalidOperationException("Empty tableau.");

                        if (cells[m.To] != null)
                            throw new InvalidOperationException("Cell not empty.");

                        var card = pile[^1];
                        pile.RemoveAt(pile.Count - 1);
                        cells[m.To] = card;
                        break;
                    }

                case MoveKind.CellToTableau:
                    {
                        RequireCell(m.From);
                        RequireTableau(m.To);

                        var card = cells[m.From];
                        if (card == null)
                            throw new InvalidOperationException("Cell empty.");

                        if (!CanPlaceOnTableau(t[m.To], card))
                            throw new InvalidOperationException("Illegal placement.");

                        cells[m.From] = null;
                        t[m.To].Add(card);
                        break;
                    }

                case MoveKind.TableauToTableau:
                    {
                        RequireTableau(m.From);
                        RequireTableau(m.To);

                        var src = t[m.From];
                        var dst = t[m.To];

                        if (src.Count == 0)
                            throw new InvalidOperationException("Empty tableau.");

                        int cnt = Math.Max(1, m.Count);
                        if (cnt > src.Count)
                            throw new InvalidOperationException("Not enough cards.");

                        // 시퀀스 유지 확인
                        for (int i = src.Count - cnt + 1; i < src.Count; i++)
                            if (!FormsSequence(src[i - 1], src[i]))
                                throw new InvalidOperationException("Sequence broken.");

                        var bottom = src[src.Count - cnt];
                        if (!CanPlaceOnTableau(dst, bottom))
                            throw new InvalidOperationException("Illegal placement.");

                        int allowed = MaxMovableSequenceCount(m.From, m.To);
                        if (cnt > allowed)
                            throw new InvalidOperationException("Sequence exceeds buffer capacity.");

                        for (int i = src.Count - cnt; i < src.Count; i++)
                            dst.Add(src[i]);

                        src.RemoveRange(src.Count - cnt, cnt);
                        break;
                    }

                case MoveKind.TableauToFoundation:
                    {
                        RequireTableau(m.From);
                        RequireFoundation(m.To);

                        var pile = t[m.From];
                        if (pile.Count == 0)
                            throw new InvalidOperationException("Empty tableau.");

                        var card = pile[^1];

                        if (SuitIndex(card.Suit) != m.To)
                            throw new InvalidOperationException("Wrong foundation.");

                        if (!CanMoveToFoundation(card))
                            throw new InvalidOperationException("Illegal foundation move.");

                        pile.RemoveAt(pile.Count - 1);
                        ftop[m.To] = (int)card.Rank;
                        break;
                    }

                case MoveKind.CellToFoundation:
                    {
                        RequireCell(m.From);
                        RequireFoundation(m.To);

                        var card = cells[m.From];
                        if (card == null)
                            throw new InvalidOperationException("Cell empty.");

                        if (SuitIndex(card.Suit) != m.To)
                            throw new InvalidOperationException("Wrong foundation.");

                        if (!CanMoveToFoundation(card))
                            throw new InvalidOperationException("Illegal foundation move.");

                        cells[m.From] = null;
                        ftop[m.To] = (int)card.Rank;
                        break;
                    }

                case MoveKind.FoundationToCell:
                    {
                        RequireFoundation(m.From);
                        RequireCell(m.To);

                        int topRank = ftop[m.From];
                        if (topRank <= 0)
                            throw new InvalidOperationException("Foundation empty.");

                        if (cells[m.To] != null)
                            throw new InvalidOperationException("Cell not empty.");

                        cells[m.To] = new CardModel((CardSuit)m.From, (CardRank)topRank, true);
                        ftop[m.From] = topRank - 1;
                        break;
                    }

                case MoveKind.FoundationToTableau:
                    {
                        RequireFoundation(m.From);
                        RequireTableau(m.To);

                        int topRank = ftop[m.From];
                        if (topRank <= 0)
                            throw new InvalidOperationException("Foundation empty.");

                        var card = new CardModel((CardSuit)m.From, (CardRank)topRank, true);
                        if (!CanPlaceOnTableau(t[m.To], card))
                            throw new InvalidOperationException("Illegal placement.");

                        t[m.To].Add(card);
                        ftop[m.From] = topRank - 1;
                        break;
                    }

                default:
                    throw new NotSupportedException("Unknown move kind: " + m.Kind);
            }

            return new ClassicState(Seed, Config, t, cells, ftop, moves);
        }

        // ------------------------------------------------------
        // MaxMovableSequenceCount — Freecell 핵심 규칙
        // ------------------------------------------------------
        public int MaxMovableSequenceCount(int src, int dst)
        {
            RequireTableau(src);
            RequireTableau(dst);

            var s = Tableaus[src];
            var d = Tableaus[dst];

            if (s.Count == 0)
                return 0;

            // 시퀀스 숫자(아래에서 위로)
            int serial = 1;
            for (int i = s.Count - 1; i - 1 >= 0; i--)
            {
                if (FormsSequence(s[i - 1], s[i]))
                    serial++;
                else break;
            }

            int freeCells = CountFreecells();
            int emptyTableaus = CountEmptyTableaus();

            bool destEmpty = d.Count == 0;
            int usableEmpties = emptyTableaus - (destEmpty ? 1 : 0);
            if (usableEmpties < 0) usableEmpties = 0;

            long cap = freeCells + 1;
            for (int i = 0; i < usableEmpties; i++)
                cap *= 2;

            int maxByBuffer = (int)Math.Min(int.MaxValue, cap);

            if (d.Count > 0)
            {
                int best = 0;
                int kMax = Math.Min(serial, maxByBuffer);

                for (int k = kMax; k >= 1; k--)
                {
                    var bottom = s[s.Count - k];
                    if (CanPlaceOnTableau(d, bottom))
                    {
                        best = k;
                        break;
                    }
                }
                return best;
            }

            return Math.Min(serial, maxByBuffer);
        }

        // ------------------------------------------------------
        // Helper Tools
        // ------------------------------------------------------
        private List<CardModel>[] CloneTableaus()
        {
            var t = new List<CardModel>[Tableaus.Length];
            for (int i = 0; i < t.Length; i++)
                t[i] = new List<CardModel>(Tableaus[i]);
            return t;
        }

        private int CountFreecells()
        {
            int cnt = 0;
            for (int i = 0; i < Cells.Length; i++)
            {
                if (Cells[i] == null)
                    cnt++;
            }
            return cnt;
        }

        private int CountEmptyTableaus()
        {
            int cnt = 0;
            for (int i = 0; i < Tableaus.Length; i++)
                if (Tableaus[i].Count == 0)
                    cnt++;
            return cnt;
        }

        private static bool FormsSequence(CardModel lower, CardModel upper)
        {
            bool alt = IsRed(lower.Suit) != IsRed(upper.Suit);
            bool rank = ((int)lower.Rank) == ((int)upper.Rank) + 1;
            return alt && rank;
        }

        private static bool CanPlaceOnTableau(List<CardModel> dest, CardModel card)
        {
            if (dest.Count == 0) return true;

            var top = dest[^1];
            bool alt = IsRed(top.Suit) != IsRed(card.Suit);
            bool rank = ((int)top.Rank) == ((int)card.Rank) + 1;

            return alt && rank;
        }

        private bool CanMoveToFoundation(CardModel card)
        {
            int idx = SuitIndex(card.Suit);
            return FoundationTop[idx] + 1 == (int)card.Rank;
        }

        private static bool IsRed(CardSuit s)
        {
            return s == CardSuit.Heart || s == CardSuit.Diamond;
        }

        private static int SuitIndex(CardSuit s)
        {
            return s switch
            {
                CardSuit.Spade => 0,
                CardSuit.Heart => 1,
                CardSuit.Club => 2,
                CardSuit.Diamond => 3,
                _ => 0
            };
        }

        private void RequireTableau(int idx)
        {
            if (idx < 0 || idx >= Tableaus.Length)
                throw new ArgumentOutOfRangeException(nameof(idx), "Invalid tableau index");
        }

        private void RequireCell(int idx)
        {
            if (idx < 0 || idx >= Cells.Length)
                throw new ArgumentOutOfRangeException(nameof(idx), "Invalid cell index");
        }

        private void RequireFoundation(int idx)
        {
            if (idx < 0 || idx >= Config.Foundations)
                throw new ArgumentOutOfRangeException(nameof(idx), "Invalid foundation index");
        }
    }
}
