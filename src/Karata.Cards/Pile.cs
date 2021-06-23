using System.Linq;
using System.Collections.Generic;

namespace Karata.Cards
{
    public class Pile
    {
        public Stack<Card> Cards { get; set; } = new();

        public Stack<Card> RemoveAllButTop()
        {
            var topCard = Cards.Pop();
            var oldStack = new Stack<Card>(Cards.Reverse());
            Cards = new Stack<Card>();
            Cards.Push(topCard);
            return oldStack;
        }
    }
}