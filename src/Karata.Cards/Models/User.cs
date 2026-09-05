using System.Text.Json.Serialization;
using Karata.Runtime.Models;

namespace Karata.Cards.Models;

public class User : KarataUser
{
    [JsonIgnore] public List<Hand> Hands { get; set; } = [];
}