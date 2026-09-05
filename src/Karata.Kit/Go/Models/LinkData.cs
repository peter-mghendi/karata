using Karata.Kit.Platform.Models;

namespace Karata.Kit.Go.Models;

public record LinkData
{
    public required string Slug { get; set; }
    public required Uri Destination { get; set; }
    public required string Application { get; set; }
    public required UserData Creator { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiredAt { get; set; }
}