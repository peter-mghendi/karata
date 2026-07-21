using Karata.Kit.Cards.Models;

namespace Karata.Runtime.Models;

public class KarataUser
{
    public required string Id { get; set; }

    public required string Username { get; set; }
    
    public virtual UserData ToData() => new()
    {
        Id = Id,
        Username = Username,
    };
}

