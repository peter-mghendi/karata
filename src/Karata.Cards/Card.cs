namespace Karata.Cards;

public partial record class Card 
{
    public CardFace Face { get; set; }
    public CardSuit Suit { get; set; }
}