using System.Text.Json.Serialization;
using Karata.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Karata.Server.Models;

public class User : IdentityUser
{
    [JsonIgnore] public List<Hand> Hands { get; set; } = [];

    public UserData ToData() => new()
    {
        Id = Id,
        Username = UserName!,
    };
}