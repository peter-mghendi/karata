using Karata.Cards;

namespace Karata.Web.Models
{
    public record User(string Username) {
        public Hand Hand { get; set; } = new();
    }
}