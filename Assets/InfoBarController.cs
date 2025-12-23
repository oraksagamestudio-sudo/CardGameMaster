using UnityEngine;

[RequireComponent(typeof(InfoBarView))]
public class InfoBarController : MonoBehaviour
{
    private InfoBarView view;
    [SerializeField] private FreecellClassicGameManager game;

    private void Awake()
    {
        view = GetComponent<InfoBarView>();
        game.OnGameStarted += Init;
    }

    private void OnDestroy() {
        game.OnGameStarted -= Init;
    }
    void Update()
    {
        view.SetTime(game.Status.ElapsedTime);
        view.SetScore(game.Status.Score);
    }

    public void Init(GameStatusModel model)
    {
        view.SetTime(model.ElapsedTime);
        view.SetScore(model.Score);
        view.SetGameNumber(model.GameNumber);
    }
}