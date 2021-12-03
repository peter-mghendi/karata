#nullable enable

using Karata.Web.Models.UI;

namespace Karata.Web.Models;

public class Hand
{
    public int Id { get; set; }
    public List<Card> Cards { get; set; } = new();
    public bool IsLastCard { get; set; } = false;
    public int GameId { get; set; }
    public virtual User? User { get; set; }

    public UIHand ToUI() => new()
    {
        Cards = Cards,
        User = User?.ToUI(),
    };
}