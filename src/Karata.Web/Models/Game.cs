using System.Collections.Generic;
using Karata.Cards;

namespace Karata.Web.Models
{
    public class Game
    {
        public Deck Deck { get; set; } = Deck.StandardDeck;
        public Pile Pile { get; set; } = new();
        public List<User> Players { get; set; } = new();
        public int CurrentTurn { get; set; } = 0;
        public bool Started { get; set; } = false;
    }
}