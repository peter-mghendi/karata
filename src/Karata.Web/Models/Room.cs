using System;
using System.Collections.Generic;

namespace Karata.Web.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string InviteLink { get; set; }
        public ApplicationUser Creator { get; set; }
        public Game Game { get; set; } = new ();
        public DateTime CreatedAt { get; } = DateTime.Now;
        public ICollection<ChatMessage> ChatMessages { get; set; }
        public ICollection<SystemMessage> SystemMessages { get; set; }
    }
}
