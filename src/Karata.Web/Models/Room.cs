using System;
using System.Collections.Generic;

namespace Karata.Web.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string InviteLink { get; set; }
        public virtual ApplicationUser Creator { get; set; }
        public virtual Game Game { get; set; } = new ();
        public DateTime CreatedAt { get; } = DateTime.Now;
        public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
    }
}
