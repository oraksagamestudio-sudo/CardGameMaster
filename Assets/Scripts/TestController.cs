//Assets/Scripts/TestController.cs
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Solitaire;
using UnityEngine.SceneManagement;
using System;

public class TestController : MonoBehaviour
{
    

    [SerializeField] private GameObject cardPrefab;
    //[SerializeField] private TableauController[] parents;          // Tableau들
    [SerializeField] private Transform generationTarget;   // 초기 생성 위치


    private bool _spriteProviderInit = false;
    private Coroutine _spawnRoutine;


    // // 버튼 다중 클릭방지
    // private bool _clickedRecently = false;
    // private IEnumerator ClickDebounce()
    // {
    //     _clickedRecently = true;
    //     yield return null;     // 1프레임
    //     _clickedRecently = false;
    // }


    // private float _cardMovingDuration = 0.5f;

    //---------------------------------------------
    // 버튼 클릭
    //---------------------------------------------
    // private int _clickCount = 0;
    public void GameStart()
    {
        // if (_clickedRecently) return;
        // StartCoroutine(ClickDebounce());
        // _clickCount++;
        // Debug.Log($"[OnTestButtonClick] Count = {_clickCount}");
        StopAllCoroutines();
        _spawnRoutine = null;
        

        // 기존 카드 오브젝트 전부 삭제
        ClearAllCards();

        // 새 코루틴 실행
        _spawnRoutine = StartCoroutine(SpawnXor());
    }

    //---------------------------------------------
    // 기존 카드 삭제
    //---------------------------------------------
    private void ClearAllCards()
    {
        var sr = SlotManager.Instance;
        var parents = sr.Tableaus;
        parents.AddRange(sr.Freecells);
        parents.AddRange(sr.Foundations);
        foreach (var slot in parents)
        {
            for (int i = slot.transform.childCount - 1; i >= 0; i--)
                Destroy(slot.transform.GetChild(i).gameObject);
            slot.Clear();
        }

        // 생성 위치(대기 위치)에 남아있는 카드가 있을 수도 있으므로 정리
        for (int i = generationTarget.childCount - 1; i >= 0; i--)
            if (generationTarget.GetChild(i).GetComponent<StaticCardController>() != null)
                Destroy(generationTarget.GetChild(i).gameObject);

        
    }
    private static uint GetRandomSeed()
    {
        uint result = (uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue)
           ^ ((uint)DateTime.Now.Ticks);
        if(result == 0) return GetRandomSeed();
        return result;
    }
        
    //---------------------------------------------
    // 카드 생성 및 이동 코루틴
    //---------------------------------------------
    private IEnumerator SpawnXor()
    {
        uint seed = GetRandomSeed();
        GameContext.StartNewClassic(seed);
        var game = GameContext.Classic;

        // var parents = SlotRegistry.Instance.Tableaus;
        var tableaus = game.State.Tableaus;
        int columnCount = tableaus.Length;

        // 각 열의 최대 카드 개수 찾기
        int maxHeight = 0;
        for (int c = 0; c < columnCount; c++)
            if (tableaus[c].Count > maxHeight)
                maxHeight = tableaus[c].Count;

        StringBuilder sb = new();

        // ---------- 핵심: 행 우선 루프 ----------
        for (int row = 0; row < maxHeight; row++)
        {
            for (int col = 0; col < columnCount; col++)
            {
                if (row >= tableaus[col].Count)
                {
                    sb.Append("   ");
                    continue; // 이 열은 이 행이 없음
                }

                var card = tableaus[col][row];
                sb.Append($"{card} ");

                // 카드 생성
                var cardObj = Instantiate(cardPrefab, generationTarget);
                var controller = cardObj.GetComponent<StaticCardController>();

                if (!_spriteProviderInit)
                {
                    CardSpriteUtility.Init(cardObj);
                    _spriteProviderInit = true;
                }

                controller.Init(card);

                // DOTween 이동 (부모 변경 없음)
                CardMotionService.Instance.MoveToSlot(
                    (RectTransform)controller.transform,
                    SlotManager.Instance.Tableaus[col].transform,
                    0.25f
                );

                yield return new WaitForSeconds(0.08f);
            }
            sb.Append("\n");
        }

        Debug.Log($"[FreecellTest] SEED = {seed}\n{sb}");

        // 코루틴 종료
        _spawnRoutine = null;
        // _clickedRecently = false;
    }


    public void LegalTest()
    {
        StringBuilder sb = new();
        foreach(var move in GameContext.Classic.State.GetLegalMoves())
        {
            sb.Append(move);
            sb.Append("\n");
        }
        Debug.Log(sb);
    }

    public void AutoFoundation()
    {
        var moves = GameContext.Classic.ApplyAutoFoundation();
        var registry = SlotManager.Instance;

        foreach (var move in moves)
        {
            SlotController from = null;
            SlotController to = null;

            switch (move.Kind)
            {
                case MoveKind.TableauToFoundation:
                    from = registry.Tableaus[move.From];
                    to = registry.Foundations[move.To];
                    break;

                case MoveKind.CellToFoundation:
                    from = registry.Freecells[move.From];
                    to = registry.Foundations[move.To];
                    break;
            }

            if (from == null || to == null)
                continue;

            var cards = from.Cards;
            if (cards.Count == 0)
                continue;

            var card = cards[cards.Count - 1];
            from.RemoveCard(card);
            to.AddCard(card);

            // SlotLayoutService.Instance.UpdateLayout(from.transform);
            // SlotLayoutService.Instance.UpdateLayout(to.transform);
        }
    }

    public void Undo()
    {
        var registry = SlotManager.Instance;

        static int CardKey(CardModel m) => (((int)m.Suit) << 8) | (int)m.Rank;

        void CollectCards(SlotController slot, Dictionary<int, StaticCardController> map)
        {
            foreach (var card in slot.Cards)
                map[CardKey(card.model)] = card;
        }

        var cardMap = new Dictionary<int, StaticCardController>(52);
        foreach (var t in registry.Tableaus) CollectCards(t, cardMap);
        foreach (var c in registry.Freecells) CollectCards(c, cardMap);
        foreach (var f in registry.Foundations) CollectCards(f, cardMap);

        if (!GameContext.Undo())
            return;

        void ClearSlot(SlotController slot)
        {
            var copy = new List<StaticCardController>(slot.Cards);
            foreach (var card in copy)
                slot.RemoveCard(card);
        }

        foreach (var t in registry.Tableaus) ClearSlot(t);
        foreach (var c in registry.Freecells) ClearSlot(c);
        foreach (var f in registry.Foundations) ClearSlot(f);

        var state = GameContext.Classic.State;

        for (int i = 0; i < state.Tableaus.Length; i++)
        {
            var slot = registry.Tableaus[i];
            foreach (var model in state.Tableaus[i])
                if (cardMap.TryGetValue(CardKey(model), out var card))
                    slot.AddCard(card);

            // SlotLayoutService.Instance.UpdateLayout(slot.transform);
        }

        for (int i = 0; i < state.Cells.Length && i < registry.Freecells.Count; i++)
        {
            var model = state.Cells[i];
            if (model != null && cardMap.TryGetValue(CardKey(model), out var card))
                registry.Freecells[i].AddCard(card);

            // SlotLayoutService.Instance.UpdateLayout(registry.Freecells[i].transform);
        }

        for (int i = 0; i < state.FoundationTop.Length && i < registry.Foundations.Count; i++)
        {
            var slot = registry.Foundations[i];
            int top = state.FoundationTop[i];
            for (int rank = 1; rank <= top; rank++)
            {
                var key = (((int)i) << 8) | rank;
                if (cardMap.TryGetValue(key, out var card))
                    slot.AddCard(card);
            }
            // SlotLayoutService.Instance.UpdateLayout(slot.transform);
        }
    }


    public void OnBack()
    {
        BackToLobbyScene();
    }

    public void BackToLobbyScene()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }
}
