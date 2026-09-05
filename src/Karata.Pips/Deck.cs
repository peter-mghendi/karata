using Karata.Pips.Extensions;
using static Karata.Pips.Card.CardColor;
using static Karata.Pips.Card.CardFace;
using static Karata.Pips.Card.CardSuit;

namespace Karata.Pips;

public class Deck : Stack<Card>
{

    public Deck(IEnumerable<Card> collection) : base(collection)
    {
    }

    public Deck()
    {
    }

    public static Deck Standard
    {
        get
        {
            var deck = new Deck();
            foreach (var suit in Enum.GetValues<Card.CardSuit>())
            {
                if (suit is BlackJoker or RedJoker) continue;
                foreach (var face in Enum.GetValues<Card.CardFace>())
                {
                    if (face is None or Joker) continue;
                    deck.Push(face.Of(suit));
                }
            }

            deck.Push(Black.Joker);
            deck.Push(Red.Joker);
            return new Deck(deck);
        }
    }

    public void ShuffleInPlace()
    {
        var cards = this.Shuffle().ToList();
        
        Clear();
        cards.ForEach(Push);
    }

    // Deal single card without checking deck size first.
    public Card Deal() => Pop();

    // Deal multiple cards without checking deck size first.
    public List<Card> DealMany(uint num) => Enumerable.Range(0, checked((int)num)).Select(_ => Deal()).ToList();

    // Check deck size before attempting to deal single card.
    public bool TryDeal(out Card? dealt) => (dealt = Count > 0 ? Deal() : null) is not null;

    // Check deck size before attempting to deal multiple cards.
    public bool TryDealMany(uint num, out List<Card> dealt) => (dealt = Count >= num ? DealMany(num) : []).Count == num;
}