using Karata.Cards;
using static Karata.Cards.Card.CardFace;

namespace Karata.Web.Extensions;

public static class CardExtensions
{
    extension(Card card)
    {
        public string ImageSource(bool faceUp = true)
        {
            if (!faceUp) return "img/cards/Back.svg";
        
            return card is { Face: Joker }
                ? $"img/cards/{card.Suit.ToString()}.svg"
                : $"img/cards/{card.Face.ToString()}{card.Suit.ToString()}.svg";
        }
    }
}