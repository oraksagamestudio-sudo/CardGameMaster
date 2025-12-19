// Assets/Scripts/Solitaire.FreecellClassic/ClassicReplayService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Solitaire.Core;
using UnityEngine;

namespace Solitaire.FreecellClassic
{
    /// <summary>
    /// Freecell Classic 전용 Replay 저장/로드/스냅샷/재적용 서비스.
    /// Plus 전용 기능은 완전히 제거된 순수 클래식 구조.
    /// </summary>
    public static class ClassicReplayService
    {
        private const string FileName = "freecell_classic_replay.json";

        private static readonly string RankMap = "a23456789tjqk"; // 1..13
        private static readonly string SuitMap = "shdc";          // 0..3

        // ----------------------------------------------
        // Save
        // ----------------------------------------------
        public static void SaveReplay(ClassicReplayFile rf)
        {
            try
            {
                string dir = Application.persistentDataPath;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string path = Path.Combine(dir, FileName);

                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                };

                string json = JsonConvert.SerializeObject(rf, settings);
                File.WriteAllText(path, json, Encoding.UTF8);

                Debug.Log($"[ClassicReplayService] Saved: {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError("[ClassicReplayService] Save failed: " + ex.Message);
            }
        }

        // ----------------------------------------------
        // Load
        // ----------------------------------------------
        public static ClassicReplayFile LoadReplay()
        {
            string path = Path.Combine(Application.persistentDataPath, FileName);

            if (!File.Exists(path))
            {
                Debug.LogWarning("[ClassicReplayService] No replay file.");
                return null;
            }

            try
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                return JsonConvert.DeserializeObject<ClassicReplayFile>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError("[ClassicReplayService] Load failed: " + ex.Message);
                return null;
            }
        }

        // ------------------------------------------------
        // Snapshot 생성
        // ------------------------------------------------
        public static ClassicReplaySnapshot MakeSnapshot(ClassicState s)
        {
            var snap = new ClassicReplaySnapshot
            {
                tableaus = new string[s.Tableaus.Length],
                cells = new string[s.Cells.Length],
                foundations = new string[4],
                moveCount = s.MoveCount
            };

            // tableaus
            for (int i = 0; i < s.Tableaus.Length; i++)
                snap.tableaus[i] = EncodePile(s.Tableaus[i]);

            // cells
            for (int i = 0; i < s.Cells.Length; i++)
            {
                var cellCard = s.Cells[i];
                snap.cells[i] = cellCard == null ? "" : EncodeCard(cellCard);
            }

            // foundations (suit 0~3)
            for (int suit = 0; suit < 4; suit++)
                snap.foundations[suit] = EncodeFoundationTop(suit, s.FoundationTop[suit]);

            return snap;
        }

        // ------------------------------------------------
        // Snapshot → ClassicState 복원
        // ------------------------------------------------
        public static ClassicState BuildFromSnapshot(uint seed, ClassicConfig cfg, ClassicReplaySnapshot snap)
        {
            if (snap == null)
                throw new Exception("Snapshot is null.");

            var tableaus = new List<CardModel>[cfg.Tableaus];
            for (int i = 0; i < cfg.Tableaus; i++)
                tableaus[i] = DecodePile(snap.tableaus[i]);

            var cells = new CardModel[cfg.Cells];
            for (int i = 0; i < cfg.Cells; i++)
                cells[i] = string.IsNullOrEmpty(snap.cells[i]) ? null : DecodeCard(snap.cells[i]);

            var ftop = new int[4];
            for (int suit = 0; suit < 4; suit++)
                ftop[suit] = DecodeFoundationRank(snap.foundations[suit], suit);

            return new ClassicState(seed, cfg, tableaus, cells, ftop, snap.moveCount);
        }

        // ------------------------------------------------
        // Replay 재적용
        // ------------------------------------------------
        public static ClassicState ReplayIntoState(ClassicReplayFile rf, out ShuffleKind shuffleUsed)
        {
            var cfg = rf.config.ToClassicConfig();

            shuffleUsed = rf.shuffleKind;
            var s = ClassicState.NewGame(rf.seed, cfg, shuffleUsed);

            // snapshot 우선
            if (rf.snapshot != null)
            {
                try
                {
                    s = BuildFromSnapshot(rf.seed, cfg, rf.snapshot);
                }
                catch
                {
                    Debug.LogWarning("[ClassicReplayService] Snapshot load failed, fallback to full replay.");
                }
            }

            // moves 재적용
            foreach (var step in rf.steps)
            {
                if (!TryParseMove(step, out var mv))
                    continue;

                try
                {
                    s = s.Apply(mv);
                }
                catch (Exception e)
                {
                    Debug.LogError("[ClassicReplayService] replay apply failed: " + e.Message);
                    break;
                }
            }

            return s;
        }

        // ------------------------------------------------
        // Step → Move 파싱
        // ------------------------------------------------
        public static bool TryParseMove(ClassicReplayStep rs, out Move mv)
        {
            mv = default;

            if (rs.kind != "move")
                return false;

            if (!Enum.TryParse<MoveKind>(rs.op, true, out var mk))
                return false;

            mv = new Move(mk, rs.from, rs.to, rs.count > 0 ? rs.count : 1);
            return true;
        }

        // ------------------------------------------------
        // Encode / Decode Helpers
        // ------------------------------------------------
        private static string EncodeCard(CardModel c)
        {
            int r = (int)c.Rank;
            int s = CardModel.SuitIndex(c.Suit);
            char rc = RankMap[r - 1];
            char sc = SuitMap[s];
            return $"{rc}{sc}";
        }

        private static CardModel DecodeCard(string code)
        {
            int r = RankMap.IndexOf(char.ToLowerInvariant(code[0])) + 1;
            int s = SuitMap.IndexOf(char.ToLowerInvariant(code[1]));
            return new CardModel((CardSuit)s, (CardRank)r);
        }

        private static string EncodePile(List<CardModel> pile)
        {
            var sb = new StringBuilder(pile.Count * 2);
            foreach (var c in pile)
                sb.Append(EncodeCard(c));

            return sb.ToString();
        }

        private static List<CardModel> DecodePile(string enc)
        {
            var list = new List<CardModel>();
            if (string.IsNullOrEmpty(enc)) return list;

            for (int i = 0; i < enc.Length; i += 2)
                list.Add(DecodeCard(enc.Substring(i, 2)));

            return list;
        }

        private static string EncodeFoundationTop(int suitIndex, int topRank)
        {
            if (topRank <= 0) return "";
            char rc = RankMap[topRank - 1];
            char sc = SuitMap[suitIndex];
            return $"{rc}{sc}";
        }

        private static int DecodeFoundationRank(string encoded, int suit)
        {
            if (string.IsNullOrEmpty(encoded))
                return 0;

            int r = RankMap.IndexOf(char.ToLowerInvariant(encoded[0])) + 1;
            int s = SuitMap.IndexOf(char.ToLowerInvariant(encoded[1]));

            if (s != suit)
                throw new Exception("foundation mismatch");

            return r;
        }
    }
}
