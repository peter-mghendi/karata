namespace Karata.Kit.Go.Models;

public record LinkRequest(Uri Destination, DateTimeOffset? ExpiresAt);