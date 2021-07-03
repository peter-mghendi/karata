using System.Linq;
using System.Collections.Generic;

namespace Karata.Cards
{
    public class Pile
    {
        public Stack<Card> Cards { get; set; } = new();

        // Empty pile but keep top card
        public Stack<Card> Reclaim()
        {
            var topCard = Cards.Pop();
            var oldStack = new Stack<Card>(Cards.Reverse());
            Cards = new Stack<Card>();
            Cards.Push(topCard);
            return oldStack;
        }
    }
}