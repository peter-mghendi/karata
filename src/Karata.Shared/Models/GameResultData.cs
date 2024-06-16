using Karata.Server.Models;

namespace Karata.Shared.Models;

public record GameResultData
{
    public UserData? Winner { get; init; }
    public required string Reason { get; init; }
    public required MessageType ReasonType { get; init; }
    public required GameResultType ResultType { get; init; }
}