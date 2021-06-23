using System;
using System.Collections.Generic;

namespace Karata.Cards.Shufflers
{
    public class FisherYatesShuffler : IShuffler
    {
        private readonly Random _random = new();
        public Stack<Card> Shuffle(Stack<Card> cards)
        {
            var cardArray = cards.ToArray();
            var i = cardArray.Length;
            while (--i > 0) {
                var j = _random.Next(i + 1);
                var temp = cardArray[j];
                cardArray[j] = cardArray[i];
                cardArray[i] = temp;
            }
            return new Stack<Card>(cardArray);
        }
    }
}