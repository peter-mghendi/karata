#nullable enable

namespace Karata.Web.Models;

public class Game
{
    public int Id { get; set; }

    public bool IsForward { get; set; } = true;
    public bool IsStarted { get; set; } = false;

    public Card? CurrentRequest { get; set; } = null;
    public uint Give { get; set; } = 0;
    public uint Pick { get; set; } = 0;
    public int CurrentTurn { get; set; } = 0;

    public Deck Deck { get; set; } = Deck.StandardDeck;
    public Pile Pile { get; set; } = new();

    public virtual List<ApplicationUser> Players { get; set; } = new();
    public virtual ApplicationUser? Winner { get; set; } = null;
    public virtual List<Hand> Hands { get; set; } = new();
    public virtual List<Turn> Turns { get; set; } = new();

    public int RoomId { get; set; }
}