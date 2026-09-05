using Karata.Kit.Platform.Models;

namespace Karata.Runtime.Models;

public class KarataUser
{
    public required string Id { get; set; }

    public required string Username { get; set; }
    
    public static implicit operator UserData(KarataUser user) => user.ToData();
    
    public virtual UserData ToData() => new()
    {
        Id = Id,
        Username = Username,
    };
}

