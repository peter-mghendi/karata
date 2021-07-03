using System.Collections.Generic;
using Karata.Cards;

namespace Karata.Web.Models
{
    public record User(string Username) {
        public List<Card> Hand { get; } = new();
    }
}