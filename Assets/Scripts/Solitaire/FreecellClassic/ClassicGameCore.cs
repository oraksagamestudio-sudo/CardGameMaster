// Assets/Scripts/Solitaire/FreecellClassic/ClassicGameService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Solitaire.Core;

namespace Solitaire.FreecellClassic
{
    public sealed class ClassicGameCore
    {
        // -------------------------
        // 기본 상태 데이터
        // -------------------------
        public ClassicState State { get; private set; }
        public ClassicConfig Config { get; private set; }
        public uint Seed { get; private set; }
        public ShuffleKind ShuffleKind { get; private set; }

        // -------------------------
        // Undo / Redo 스택
        // -------------------------
        private readonly Stack<ClassicState> _undo = new();
        private readonly Stack<ClassicState> _redo = new();

        // -------------------------
        // 생성 및 초기화
        // -------------------------
        public ClassicGameCore(uint seed, ClassicConfig cfg = null, ShuffleKind sk = ShuffleKind.Xor)
        {
            Seed = seed;
            Config = cfg ?? ClassicConfig.Default;
            ShuffleKind = sk;

            State = ClassicState.NewGame(seed, Config, sk);
            _undo.Clear();
            _redo.Clear();
        }

        // -------------------------
        // Move
        // -------------------------
        public bool Move(MoveKind kind, int from, int to, int count = 1)
        {
            var mv = new Move(kind, from, to, count);
            try
            {
                // 상태 저장 후 적용
                _undo.Push(State);
                _redo.Clear();

                State = State.Apply(mv);
                return true;
            }
            catch (Exception e)
            {

                var tableaus = State.Tableaus;
                int columnCount = tableaus.Length;

                // 각 열의 최대 카드 개수 찾기
                int maxHeight = 0;
                for (int c = 0; c < columnCount; c++)
                    if (tableaus[c].Count > maxHeight)
                        maxHeight = tableaus[c].Count;

                StringBuilder sb = new();

                for (int row = 0; row < maxHeight; row++)
                {
                    for (int col = 0; col < columnCount; col++)
                    {
                        var tableau = tableaus[col];
                        if (row >= tableau.Count)
                            continue; // 이 열은 이 행이 없음

                        var card = tableau[row];
                        sb.Append($"{(card != null?card:"  ")} ");
                    }
                    sb.Append("\n");
                }
                UnityEngine.Debug.LogWarning($"[Classic] Move failed: {e.Message}\n{mv}\n{sb}");
                return false;
            }
        }

        // -------------------------
        // Undo
        // -------------------------
        public bool Undo()
        {
            if (_undo.Count == 0)
                return false;

            _redo.Push(State);
            State = _undo.Pop();
            return true;
        }

        // -------------------------
        // Redo
        // -------------------------
        public bool Redo()
        {
            if (_redo.Count == 0)
                return false;

            _undo.Push(State);
            State = _redo.Pop();
            return true;
        }

        // -------------------------
        // Hint
        // -------------------------
        public struct ScoredMove
        {
            public Move move;
            public int score;
            public string reason;
        }

        public List<ScoredMove> Hint(int topN = 5)
        {
            return GetScoreMoves(State)
                .OrderByDescending(x => x.score)
                .Take(topN)
                .ToList();
        }

        private IEnumerable<ScoredMove> GetScoreMoves(ClassicState s)
        {
            foreach (var m in s.GetLegalMoves())
            {
                yield return GetScore(m);
            }
        }

        private ScoredMove GetScore(Move m)
        {
            int score = 0;
            var reasons = new List<string>();

            switch (m.Kind)
            {
                case MoveKind.TableauToFoundation:
                    score += 1000;
                    reasons.Add("to-foundation");
                    break;

                case MoveKind.CellToFoundation:
                    score += 1100;
                    reasons.Add("from-cell-to-foundation");
                    break;

                case MoveKind.TableauToCell:
                    score -= 200;
                    reasons.Add("fills-cell");
                    break;

                case MoveKind.CellToTableau:
                    score += 200;
                    reasons.Add("empties-cell");
                    break;

                case MoveKind.TableauToTableau:
                    score += 220;
                    reasons.Add("build");
                    break;
            }

            return new ScoredMove
            {
                move = m,
                score = score,
                reason = string.Join(",", reasons)
            };
        }

        // -------------------------
        // Auto Foundation
        // -------------------------
        public List<Move> ApplyAutoFoundation()
        {
            var moves = new List<Move>();

            while (true)
            {
                var mv = State.GetLegalMoves()
                    .FirstOrDefault(m =>
                        m.Kind == MoveKind.TableauToFoundation ||
                        m.Kind == MoveKind.CellToFoundation);

                if (mv == null || mv.Kind == MoveKind.None)
                    break;

                _undo.Push(State);
                _redo.Clear();

                State = State.Apply(mv);
                moves.Add(mv);
            }

            return moves;
        }

        // -------------------------
        // Auto Foundation Simulation
        // -------------------------
        public bool WouldAutoFoundationWin()
        {
            var sim = State;

            while (true)
            {
                var mv = sim.GetLegalMoves()
                    .FirstOrDefault(m =>
                        m.Kind == MoveKind.TableauToFoundation ||
                        m.Kind == MoveKind.CellToFoundation);

                if (mv == null || mv.Kind == MoveKind.None)
                    break;

                sim = sim.Apply(mv);
            }

            return sim.IsVictory;
        }

        // -------------------------
        // Replay → GameState 로드
        // -------------------------
        public void LoadFromReplay(ClassicState s, uint seed, ShuffleKind sk)
        {
            Seed = seed;
            ShuffleKind = sk;

            _undo.Clear();
            _redo.Clear();
            State = s;
        }

        // internal bool CanMove(MoveKind mk, int index1, int index2, int count)
        // {
        //     var m = new Move(
        //         kind: mk,
        //         from: index1, 
        //         to: index2,
        //         count: count
        //     );

        //     var hints = Hint();
        //     foreach(var hint in hints)
        //     {
        //         if(hint.move.Equals(m)) return true;
        //     }
        //     return false;
        // }
        // internal bool CanMove(MoveKind mk, int index1, int index2, int count)
        // {
        //     var request = new Move(mk, index1, index2, count);

        //     foreach (var legal in State.GetLegalMoves())
        //     {
        //         // Count가 1일 때도 many-move legal move 비교를 위해
        //         if (legal.From == request.From &&
        //             legal.To == request.To &&
        //             legal.Kind == request.Kind &&
        //             request.Count <= legal.Count)
        //         {
        //             return true;
        //         }
        //     }

        //     return false;
        // }
    }
}
