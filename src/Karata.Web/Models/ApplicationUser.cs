using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Karata.Cards;
using Microsoft.AspNetCore.Identity;

namespace Karata.Web.Models
{
    public class ApplicationUser : IdentityUser {
        [NotMapped]
        public List<Card> Hand { get; set; } = new();
        [NotMapped]
        public List<Room> CreatedRooms { get; set; } = new();
    }
}