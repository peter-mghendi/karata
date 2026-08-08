using Karata.Kit.Cards.Models;

namespace Karata.Platform.Models;

public class User
{
    public required string Id { get; set; }

    public required string Username { get; set; }

    public UserData ToData() => new()
    {
        Id = Id,
        Username = Username,
    };
}