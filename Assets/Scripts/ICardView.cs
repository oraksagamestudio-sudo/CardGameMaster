// Assets/Resources/Scripts/ICardView.cs

public interface ICardView
{
    public void SetFace(bool faceUp);
    public void SetRank(CardRank rank);
    public void SetSuit(CardSuit suit);
    public void SetCardData(CardModel model);
    public void SetCardData(CardSuit suit, CardRank rank, bool faceUp = false);
}