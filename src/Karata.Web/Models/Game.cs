using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Karata.Cards;
using static Karata.Cards.Card;

namespace Karata.Web.Models
{
    public partial class Game
    {
        public int Id { get; set; }

        public bool IsStarted { get; set; } = false;
        public bool IsForward { get; set; } = true;

        public Card CurrentRequest { get; set; } = null;
        public uint Pick { get; set; } = 0;

        public virtual Deck Deck { get; set; } = Deck.StandardDeck;
        public virtual Pile Pile { get; set; } = new();

        public virtual List<ApplicationUser> Players { get; set; } = new();
        public int CurrentTurn { get; set; } = 0;

        public int RoomId { get; set; }
    }
}