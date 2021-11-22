#nullable enable

namespace Karata.Web.Models;

// This class is used to hold the player's game information.
// Decouples ApplicationUser from Game.
// Enables particpation in multiple simultaneous games.
public class Hand
{
    public List<Card> Cards { get; set; } = new();
    public bool IsLastCard { get; set; } = false;
    public int GameId { get; set; }
    public int PlayerId { get; set; }
}