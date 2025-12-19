//Assets/Scripts/Solitaire/GameContext.cs
using System.Collections.Generic;
using Solitaire.FreecellClassic;
using UnityEngine;
namespace Solitaire 
{
    public enum GameMode
    {
        FreecellClassic,
        FreecellPlus,
        Klondike,
        Spider
    }
    public static class GameContext
    {
        public static GameMode CurrentMode { get; private set; }
        public static ClassicGameCore Classic { get; private set; }

        public static uint Seed { get; private set; }

        // 새 게임 시작
        public static void StartNewClassic(uint seed)
        {
            CurrentMode = GameMode.FreecellClassic;
            Seed = seed;
            Classic = new ClassicGameCore(seed);

            Debug.Log($"[GameContext] Start Freecell Classic, Seed={seed}");
        }

        // 같은 Seed로 리스타트
        public static void Restart()
        {
            if (CurrentMode == GameMode.FreecellClassic)
            {
                Classic = new ClassicGameCore(Seed);
                Debug.Log($"[GameContext] Restart Classic, Seed={Seed}");
            }
        }

        // public static bool CanMove(SlotController from, SlotController to, int count)
        // {
        //     MoveKind mk = MoveClassifier.Classify(from.Model, to.Model);
        //     if (mk == MoveKind.None) return false;

        //     return Classic.CanMove(mk, from.Model.Index, to.Model.Index, count);
        // }

        public static bool ApplyMove(SlotController from, SlotController to, int count)
        {
            MoveKind mk = MoveClassifier.Classify(from.Model, to.Model);
            return Classic.Move(mk, from.Model.Index, to.Model.Index, count);
        }

        // Undo
        public static bool Undo()
        {
            if (CurrentMode == GameMode.FreecellClassic)
                return Classic.Undo();

            return false;
        }

        // Redo
        public static bool Redo()
        {
            if (CurrentMode == GameMode.FreecellClassic)
                return Classic.Redo();

            return false;
        }
    }

}