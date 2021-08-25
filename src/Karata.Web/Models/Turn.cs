using System;
using System.Collections.Generic;
using Karata.Cards;

namespace Karata.Web.Models
{
    // This class is used to hold the turn information.
    // This is the key to the game "replay" feature.
    // Work in progress, do not use yet.
    [Obsolete("Work in progress, do not use yet.")]
    public class Turn
    {
        public List<Card> Cards { get; set; }
        public bool IsLastCard { get; set; } = false;
        public Card Request { get; set; } = null;
        public int GameId { get; set; }
        public int PlayerId { get; set; }
    }
}