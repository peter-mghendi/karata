using System.Collections.Immutable;
using Karata.Cards.Extensions;
using static Karata.Cards.Card;
using static Karata.Cards.Card.CardColor;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;

namespace Karata.Cards;

public class Deck : Stack<Card>
{
    private readonly Random _random = new();

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
            foreach (var suit in Enum.GetValues<CardSuit>())
            {
                if (suit is BlackJoker or RedJoker) continue;
                foreach (var face in Enum.GetValues<CardFace>())
                {
                    if (face is None or Joker) continue;
                    deck.Push(face.Of(suit));
                }
            }

            deck.Push(Black.ColoredJoker());
            deck.Push(Red.ColoredJoker());
            return new Deck(deck);
        }
    }

    public void Shuffle()
    {
        var cards = ToArray();
        for (var i = cards.Length - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }
        
        Clear();
        foreach(var card in cards) Push(card);
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