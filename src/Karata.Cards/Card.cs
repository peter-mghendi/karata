namespace Karata.Cards;

public partial record Card 
{
    public CardFace Face { get; set; }
    public CardSuit Suit { get; set; }
}