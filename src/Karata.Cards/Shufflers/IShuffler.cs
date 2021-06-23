using System.Collections.Generic;
namespace Karata.Cards.Shufflers
{
    public interface IShuffler
    {
        public Stack<Card> Shuffle(Stack<Card> cards);
    }
}