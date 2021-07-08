using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Karata.Cards;
using Microsoft.AspNetCore.Identity;

namespace Karata.Web.Models
{
    public class ApplicationUser : IdentityUser {
        [NotMapped]
        public List<Card> Hand { get; set; } = new();
        public List<Room> CreatedRooms { get; set; } = new();
        public ICollection<ChatMessage> SendMessages { get; set; }
    }
}