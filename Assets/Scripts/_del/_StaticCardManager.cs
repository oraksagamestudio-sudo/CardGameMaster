using UnityEngine;

public class StaticCardManager : MonoBehaviour
{
    public CardModel model;
    public StaticCardController controller;
    public StaticCardView view;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        controller = GetComponent<StaticCardController>();
        view = GetComponent<StaticCardView>();
        model = new CardModel("");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
