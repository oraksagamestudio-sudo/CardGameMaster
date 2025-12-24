using UnityEngine;

[RequireComponent(typeof(InfoBarView))]
public class InfoBarController : MonoBehaviour
{
    private InfoBarView view;
    [SerializeField] private FreecellClassicGameManager game;
    private bool initialized = false;

    private void Awake()
    {
        view = GetComponent<InfoBarView>();
        game.OnStatusInitialized += Init;
        game.OnScoreChangedEvent += OnScoreChanged;
        game.OnCoinsChangedEvent += OnCoinsChanged;
    }

    private void OnDestroy() {
        game.OnStatusInitialized -= Init;
        game.OnScoreChangedEvent -= OnScoreChanged;
        game.OnCoinsChangedEvent -= OnCoinsChanged;
    }
    void Update()  
    {
        if (!initialized) return;

        view.SetTime(game.Status.ElapsedTime);
        
    }

    public void Init(GameStatusModel model)
    {

        view.SetTime(model.ElapsedTime);
        view.SetScore(model.Score);
        view.SetGameNumber(model.GameNumber);
        initialized = true;
    }

    private void OnScoreChanged(int score)
    {
        view.SetScore(score);
    }

    private void OnCoinsChanged(int coins)
    {
        view.SetCoins(coins);
    }
}
