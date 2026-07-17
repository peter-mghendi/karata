namespace Karata.Trivia.Models;

public class Round
{
    public long Id { get; set; }

    public int Index { get; set; }

    public Game Game { get; set; } = null!;

    public required Question Question { get; set; }

    public List<Response> Responses { get; } = [];
}