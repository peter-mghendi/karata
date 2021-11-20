namespace Karata.Web.Models;

// This class is used to hold the turn information.
// This is the key to the game "replay" feature.
// Acts like a "delta" to Hand/Game.
// Work in progress, do not use yet.
public class Turn
{
    public List<Card> Cards { get; set; }
    public int GameId { get; set; }
    public int PlayerId { get; set; }
}