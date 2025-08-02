namespace Karata.Shared.Models;

public record GameResultData
{
    public UserData? Winner { get; init; }
    public required string Reason { get; init; }
    public required MessageType ReasonType { get; init; }
    public required GameResultType ResultType { get; init; }
    public required DateTimeOffset CompletedAt { get; init; }
    
    public static GameResultData DeckExhaustion()
    {
        return new GameResultData
        {
            Reason = "There aren't enough cards left to pick.",
            ReasonType = MessageType.Error,
            ResultType = GameResultType.DeckExhaustion,
            CompletedAt = DateTimeOffset.UtcNow
        };
    }

    public static GameResultData Win(UserData winner)
    {
        return new GameResultData
        {
            Winner = winner,
            Reason = $"{winner.Username} won.",
            ReasonType = MessageType.Success,
            ResultType = GameResultType.Win,
            CompletedAt = DateTimeOffset.UtcNow
        };
    }

    public GameResultData ToData() => new()
    {
        Reason = Reason,
        ReasonType = ReasonType,
        ResultType = ResultType,
        CompletedAt = DateTimeOffset.UtcNow
    };
}