namespace Karata.Shared.Models;

public class UIHand
{
    public List<Card> Cards { get; set; } = new();
    public UIUser User { get; set; } = null!;
}