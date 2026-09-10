namespace Karata.Trivia.Models;

public class Response
{
    public long Id { get; set; }

    public int TimeLeft { get; set; }
    
    public int Score { get; set; }
    
    public required DateTimeOffset SubmittedAt { get; init; }

    public long ChoiceId { get; set; }

    public long QuestionId { get; set; }
    
    public long RoundId { get; set; }

    public Guid GameId { get; set; }
    
    public string UserId { get; set; } = null!;

    public Choice? Choice { get; set; }

    public required Round Round { get; set; }
    
    public required User User { get; set; }
}