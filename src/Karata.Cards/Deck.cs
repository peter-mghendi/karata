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

    public Deck() : base()
    {
    }

    public static Deck StandardDeck
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
        var cardArray = ToArray();
        var i = cardArray.Length;
        while (--i > 0) {
            var j = _random.Next(i + 1);
            (cardArray[i], cardArray[j]) = (cardArray[j], cardArray[i]);
        }

        Clear();
        foreach(var card in cardArray) Push(card);
    }

    // Deal single card without checking deck size first.
    public Card Deal() => Pop();

    // Deal multiple cards without checking deck size first.
    public List<Card> DealMany(uint num)
    {
        var dealt = new List<Card>();
        for (int i = 0; i < num; i++)
            dealt.Add(Deal());

        return dealt;
    }

    // Check deck size before attempting to deal single card.
    public bool TryDeal(out Card? dealt)
    {
        dealt = default;
        if (Count <= 0)
            return false;

        dealt = Deal();
        return true;
    }

    // Check deck size before attempting to deal multiple cards.
    public bool TryDealMany(uint num, out List<Card> dealt)
    {
        dealt = new();
        if (Count < num)
            return false;

        dealt = DealMany(num);
        return true;
    }
}