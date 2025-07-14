using Karata.Shared.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace Karata.Server.Models;

public class GameResult
{
    public int Id { get; set; }

    public User? Winner { get; init; }
    public required string Reason { get; init; }
    public required MessageType ReasonType { get; init; }
    public required GameResultType ResultType { get; init; }
    public required DateTimeOffset CompletedAt { get; init; }
    public int GameId { get; set; }

    public static GameResult DeckExhaustion()
    {
        return new GameResult
        {
            Reason = "There aren't enough cards left to pick.",
            ReasonType = MessageType.Error,
            ResultType = GameResultType.DeckExhaustion,
            CompletedAt = DateTimeOffset.UtcNow
        };
    }

    public static GameResult Win(User winner)
    {
        return new GameResult
        {
            Winner = winner,
            Reason = $"{winner.UserName} won.",
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