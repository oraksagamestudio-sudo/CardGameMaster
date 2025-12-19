// Assets/Scripts/Solitaire.FreecellClassic/ClassicReplayModels.cs
using System;
using System.Collections.Generic;

namespace Solitaire.FreecellClassic
{
    public sealed class ClassicReplayFile
    {
        public int version = 1;
        public uint seed;
        public ShuffleKind shuffleKind;

        public ClassicReplayConfig config;
        public List<ClassicReplayStep> steps = new();
        public ClassicReplaySnapshot snapshot;

        public string createdAt = DateTime.UtcNow.ToString("O");
        public string note;
        public Dictionary<string, string> metadata;
    }

    public sealed class ClassicReplayConfig
    {
        public int cells;
        public int foundations;
        public int tableaus;
        public bool allowSequenceMoves;

        public ClassicReplayConfig() { }
        public ClassicReplayConfig(ClassicConfig cfg)
        {
            cells = cfg.Cells;
            foundations = cfg.Foundations;
            tableaus = cfg.Tableaus;
            allowSequenceMoves = cfg.AllowSequenceMoves;
        }

        public ClassicConfig ToClassicConfig()
            => new ClassicConfig(cells, foundations, tableaus, allowSequenceMoves);
    }

    public sealed class ClassicReplayStep
    {
        public string kind;    // "move" only (classic 모드)
        public string op;      // MoveKind
        public int from;
        public int to;
        public int count;
    }

    public sealed class ClassicReplaySnapshot
    {
        public string[] tableaus;
        public string[] cells;
        public string[] foundations;
        public int moveCount;
    }
}