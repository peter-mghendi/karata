#nullable enable

namespace Karata.Web.Models.UI;

public class UIHand
{
    public List<Card> Cards { get; set; } = new();
    public UIUser? User { get; set; }
}