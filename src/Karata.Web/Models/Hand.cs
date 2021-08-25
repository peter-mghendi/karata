using System;
using System.Collections.Generic;
using Karata.Cards;

namespace Karata.Web.Models
{
    // Work in progress, do not use yet.
    [Obsolete("Work in progress, do not use yet.")]
    public class Hand
    {
        public List<Card> Cards { get; set; }
        public bool IsLastCard { get; set; } = false;
        public int GameId { get; set; }
        public int PlayerId { get; set; }
    }
}
