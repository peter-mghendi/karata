using Karata.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Karata.Server.Models;

public class User : IdentityUser {
    public List<Hand> Hands { get; set; } = new();
    public List<Turn> Turns { get; set; } = new();

    public UserData ToData() => new () 
    {
        Id = Id,
        Email = Email!,
    };
}