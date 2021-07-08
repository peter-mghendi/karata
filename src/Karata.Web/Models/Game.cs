using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Karata.Cards;

namespace Karata.Web.Models
{
    public class Game
    {
        public int Id { get; set; }
        [NotMapped]
        public Deck Deck { get; set; } = Deck.StandardDeck;
        [NotMapped]
        public Pile Pile { get; set; } = new();
        public List<ApplicationUser> Players { get; set; } = new();
        public int CurrentTurn { get; set; } = 0;
        public bool Started { get; set; } = false;
        public int RoomId { get; set; }
        public Room Room { get; set; }
    }
}