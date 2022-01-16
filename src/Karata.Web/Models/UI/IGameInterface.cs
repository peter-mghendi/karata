using Microsoft.AspNetCore.Components;

namespace Karata.Web.Models.UI;

public interface IGameInterface
{
    [Parameter]
    public UIGame Game { get; set; }

    [Parameter]
    public UIHand Hand { get; set; }

    [Parameter]
    public List<Card> Turn { get; set; }

    [Parameter]
    public EventCallback<Card> OnAddCardToTurn { get; set; }
        
    [Parameter]
    public EventCallback<Card> OnRemoveCardFromTurn { get; set; }
}


