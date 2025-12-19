using System;
using System.Collections;
using System.Text;
using Solitaire;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FreecellClassicGameManager : MonoBehaviour
{
    public static FreecellClassicGameManager Instance{ get;private set; }

    private bool _cardSpriteUtilityInit = false;
    private Coroutine _spawnRoutine;

    private Transform _generationTarget;

    private GameObject _cardPrefab;

    // private void OnEnable()
    // {
    //     _generationTarget = FreecellClassicLayoutManager.Instance.generationTarget;
    //     _cardPrefab = FreecellClassicLayoutManager.Instance.cardPrefab;
    // }
    // private int _lastHandledSceneHandle = -1;

    // private void OnEnable()
    // {
    //     SceneManager.sceneLoaded += OnSceneLoaded;
    // }

    // private void Start()
    // {
    //     HandleScene(SceneManager.GetActiveScene());
    // }

    // private void OnDisable()
    // {
    //     SceneManager.sceneLoaded -= OnSceneLoaded;
    // }

    // private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     HandleScene(scene);
    // }

    // private void HandleScene(Scene scene)
    // {
    //     if (_lastHandledSceneHandle == scene.handle)
    //         return;

    //     _lastHandledSceneHandle = scene.handle;
    //     //var controller = Object.FindAnyObjectByType<TestController>();
    //     controller.GameStart();
    // }

    public void GameStart()
    {

        _generationTarget = FreecellClassicLayoutManager.Instance.generationTarget;
        _cardPrefab = FreecellClassicLayoutManager.Instance.cardPrefab;


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
        for (int i = _generationTarget.childCount - 1; i >= 0; i--)
            if (_generationTarget.GetChild(i).GetComponent<StaticCardController>() != null)
                Destroy(_generationTarget.GetChild(i).gameObject);


    }

    //---------------------------------------------
    // 카드 생성 및 이동 코루틴
    //---------------------------------------------

    public static uint GetSeed()
    {
        if (FreecellLaunchParams.HasSeed)
        {
            FreecellLaunchParams.HasSeed = false;
            return FreecellLaunchParams.Seed;
        }
        throw new ArgumentNullException("no has seed. do not game start without the seed.");
    }
    private IEnumerator SpawnXor()
    {
        uint seed = GetSeed();
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
                var cardObj = Instantiate(_cardPrefab, _generationTarget);
                var controller = cardObj.GetComponent<StaticCardController>();

                if (!_cardSpriteUtilityInit)
                {
                    CardSpriteUtility.Init(cardObj);
                    _cardSpriteUtilityInit = true;
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
    }
}
