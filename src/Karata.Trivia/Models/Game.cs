using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations.Schema;
using Karata.Trivia.Data;

namespace Karata.Trivia.Models;

public class Game
{
    public long Id { get; set; }
    
    public required Guid Identifier { get; set; }

    public required Topic Topic { get; set; }

    public required User PlayerOne { get; set; }

    public required User PlayerTwo { get; set; }

    public List<Round> Rounds { get; init; } = [];

    [NotMapped] public FrozenSet<User> Players => [PlayerOne, PlayerTwo];
}