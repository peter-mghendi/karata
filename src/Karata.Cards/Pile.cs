namespace Karata.Cards;

public class Pile : Stack<Card>
{
    public Pile() : base()
    {
    }

    public Pile(IEnumerable<Card> collection) : base(collection)
    {
    }

    // Empty pile but keep top card
    public Stack<Card> Reclaim()
    {
        var topCard = Pop();
        var oldStack = new Stack<Card>(this);
        
        Clear();
        Push(topCard);
        return oldStack;
    }
}