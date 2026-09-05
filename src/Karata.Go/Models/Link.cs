using Karata.Kit.Go.Models;

namespace Karata.Go.Models;

public class Link
{
    public required string Slug { get; set; }
    public required Uri Destination { get; set; }
    public required long Attempts { get; set; }
    public required string Application { get; set; }
    public required User Creator { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public LinkData ToData() => new()
    {
        Slug = Slug,
        Destination = Destination,
        Application = Application,
        Creator = Creator,
        CreatedAt = CreatedAt,
        DeletedAt = DeletedAt,
    };
}