using Karata.Shared.Models;

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

    public GameResultData ToData() => new()
    {
        Reason = Reason,
        ReasonType = ReasonType,
        ResultType = ResultType,
        CompletedAt = DateTimeOffset.UtcNow
    };
}