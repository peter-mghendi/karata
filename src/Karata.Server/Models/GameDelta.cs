using static Karata.Server.Models.GameRequestLevel;

namespace Karata.Server.Models;

public record class GameDelta
{
    /**
     * <summary>Whether the turn should reverse the order of play.</summary>
     */
    public bool Reverse { get; set; }
    
    /**
     * <summary>How many turn should be skipped.</summary>
     */
    public uint Skip { get; set; } = 1;
    
    /**
     * <summary>Whether the cards imply a request for a card, and what kind of request it is.</summary>
     */
    public GameRequestLevel RequestLevel { get; set; } = NoRequest;

    /**
     * <summary>How many request levels are removed by this turn.</summary>
     */
    public uint RemoveRequestLevels { get; set; }
    
    /**
     * <summary>
     * The number of cards to give to the next player.
     * Used to determine if the next player has to draw cards.
     * </summary>
     */
    public uint Give { get; set; } = 0;
    
    /**
     * <summary>
     * The number of cards the current player has to pick up.
     * Used for empty turns, questions without answers, and bombs that are not forwarded.
     * </summary>
     */
    public uint Pick { get; set; } = 0;
}