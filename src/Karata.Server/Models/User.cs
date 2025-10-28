using System.Text.Json.Serialization;
using Karata.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Karata.Server.Models;

public class User
{
    public required string Id { get; set; }

    public required string Username { get; set; }
    
    [JsonIgnore] public List<Hand> Hands { get; set; } = [];

    public UserData ToData() => new()
    {
        Id = Id,
        Username = Username,
    };

    public static implicit operator UserData(User user) => user.ToData();
}