namespace Karata.Trivia.Models.Configuration;

public class MailSettings
{
    public required string Server { get; init; }
    public int Port { get; init; }
    public required string SenderName { get; init; }
    public required string SenderEmail { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
}