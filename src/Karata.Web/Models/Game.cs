using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Karata.Cards;

namespace Karata.Web.Models
{
    public class Game
    {
        public int Id { get; set; }
        public virtual Deck Deck { get; set; } = Deck.StandardDeck;
        public virtual Pile Pile { get; set; } = new();
        public virtual List<ApplicationUser> Players { get; set; } = new();
        public int CurrentTurn { get; set; } = 0;
        public bool Started { get; set; } = false;
        public int RoomId { get; set; }
    }
}