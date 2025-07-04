using Karata.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Karata.Server.Models;

public class User : IdentityUser {
    public List<Hand> Hands { get; set; } = [];

    public UserData ToData() => new () 
    {
        Id = Id,
        Email = Email!,
    };
}