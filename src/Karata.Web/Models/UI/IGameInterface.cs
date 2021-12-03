using Microsoft.AspNetCore.Components;

namespace Karata.Web.Models.UI;

public interface IGameInterface
{
    [Parameter]
    public UIGame Game { get; set; }

    [Parameter]
    public UIHand Hand { get; set; }

    [Parameter]
    public EventCallback<List<Card>> OnPerformTurn { get; set; }

    public void NotifyTurnPerformed();
}


