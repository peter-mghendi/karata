using static Karata.Shared.Models.CardRequestLevel;

namespace Karata.Shared.Models;

public record GameDelta
{
    /**
     * <summary>Cards to be added to the game.</summary>
     */
    public List<Card> Cards { get; init; } = [];

    /**
     * <summary>Whether the turn should reverse the order of play.</summary>
     */
    public bool Reverse { get; init; }

    /**
     * <summary>How many turn should be skipped.</summary>
     */
    public uint Skip { get; init; } = 1;

    /**
     * <summary>Whether the cards imply a request for a card, and what kind of request it is.</summary>
     */
    public CardRequestLevel RequestLevel { get; init; } = NoRequest;

    /**
     * <summary>How many request levels are removed by this turn.</summary>
     */
    public uint RemoveRequestLevels { get; init; }

    /**
     * <summary>
     * The number of cards to give to the next player.
     * Used to determine if the next player has to draw cards.
     * </summary>
     */
    public uint Give { get; init; }

    /**
     * <summary>
     * The number of cards the current player has to pick up.
     * Used for empty turns, questions without answers, and bombs that are not forwarded.
     * </summary>
     */
    public uint Pick { get; init; }
    
    public override string ToString()
    {
        return $$"""
               GameDelta {
                 Cards                = [{{string.Join(", ", Cards.Select(c => c.ToString()))}}]
                 Reverse              = {{Reverse}}
                 Skip                 = {{Skip}}
                 RequestLevel         = {{RequestLevel}}
                 RemoveRequestLevels  = {{RemoveRequestLevels}}
                 Give                 = {{Give}}
                 Pick                 = {{Pick}}
               }
               """;
    }

    public virtual bool Equals(GameDelta? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Cards.SequenceEqual(other.Cards) &&
               Reverse == other.Reverse &&
               Skip == other.Skip &&
               RequestLevel == other.RequestLevel &&
               RemoveRequestLevels == other.RemoveRequestLevels &&
               Give == other.Give &&
               Pick == other.Pick;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Cards, Reverse, Skip, (int)RequestLevel, RemoveRequestLevels, Give, Pick);
    }
}