namespace Karata.Cards;

public class Pile : Stack<Card>
{
    public Pile()
    {
    }

    public Pile(IEnumerable<Card> collection) : base(collection)
    {
    }

    // Empty pile but keep top card
    public Stack<Card> Reclaim()
    {
        var top = Pop();
        var cards = new Stack<Card>(this);
        
        Clear();
        Push(top);
        
        return cards;
    }
    
    public void Add(Card card) => Push(card);
}