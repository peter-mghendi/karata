using System;
using System.Collections.Generic;

namespace Karata.Web.Models
{
    public class Room
    {
        public string Link { get; set; }
        public ApplicationUser Creator { get; set; }
        public Game Game { get; set; } = new ();
        public List<ChatMessage> Messages = new();
        public DateTime CreatedAt { get; } = DateTime.Now;
    }
}
