#pragma warning disable BL0007
// TODO: Why do I need this?

using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using Karata.Shared.Models;
using Karata.Cards;

namespace Karata.Client.Models;

public interface IGameInterface
{
    [Parameter]
    public UIGame Game { get; set; }

    [Parameter]
    public UIHand Hand { get; set; }

    [Parameter]
    public ImmutableList<Card> Turn { get; set; }

    [Parameter]
    public EventCallback<ValueTuple<Card, int>> OnAddCardToTurn { get; set; }
    
    [Parameter]
    public EventCallback<ValueTuple<Card, int>> OnReorderCardInTurn { get; set; }

    [Parameter]
    public EventCallback<Card> OnRemoveCardFromTurn { get; set; }
}


