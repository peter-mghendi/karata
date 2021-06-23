using System;
using System.Collections.Generic;
using System.Linq;

namespace Karata.Cards.Shufflers
{
    public class OrderByRandomShuffler : IShuffler
    {
        private readonly Random _random = new();

        public Stack<Card> Shuffle(Stack<Card> cards) => new(cards.OrderBy(_ => _random.Next()));
    }
}