using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;

namespace Karata.Web.Models.UI;

public interface IGameInterface
{
    [Parameter]
    public UIGame Game { get; set; }

    [Parameter]
    public UIHand Hand { get; set; }

    [Parameter]
    public ImmutableList<Card> Turn { get; set; }

    [Parameter]
    public EventCallback<Card> OnAddCardToTurn { get; set; }
    
    [Parameter]
    public EventCallback<(Card Card, int Index)> OnReorderCardInTurn { get; set; }

    [Parameter]
    public EventCallback<Card> OnRemoveCardFromTurn { get; set; }
}


