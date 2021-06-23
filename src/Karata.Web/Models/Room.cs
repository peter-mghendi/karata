using System;
using System.Collections.Generic;

namespace Karata.Web.Models
{
    public class Room
    {
        public string Link { get; set; }

        public User Creator { get; set; }

        public Game Game { get; set; } = new ();

        public DateTime CreatedAt { get; } = DateTime.Now;

        // public List<User> Members { get => Game.Players; set; } = new();
    }
}
