using System.Text.Json.Serialization;
using Karata.Kit.Cards.Models;
using Karata.Runtime.Models;

namespace Karata.Cards.Models;

public class User : KarataUser
{
    [JsonIgnore] public List<Hand> Hands { get; set; } = [];

    public static implicit operator UserData(User user) => user.ToData();
}