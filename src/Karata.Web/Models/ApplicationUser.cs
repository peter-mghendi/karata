using System.Collections.Generic;
using Karata.Cards;
using Microsoft.AspNetCore.Identity;

namespace Karata.Web.Models
{
    public class ApplicationUser : IdentityUser {
        public List<Card> Hand { get; set; } = new();
    }
}