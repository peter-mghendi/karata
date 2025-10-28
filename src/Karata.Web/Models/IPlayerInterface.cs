#pragma warning disable BL0007

using System.Collections.Immutable;
using Karata.Cards;
using Karata.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace Karata.Web.Models;

public interface IPlayerInterface
{
    [Parameter]
    public GameData Game { get; set; }

    [Parameter]
    public HandData Hand { get; set; }

    [Parameter]
    public ImmutableList<Card> Turn { get; set; }

    [Parameter]
    public EventCallback<ValueTuple<Card, int>> OnAddCardToTurn { get; set; }
    
    [Parameter]
    public EventCallback<ValueTuple<Card, int>> OnReorderCardInTurn { get; set; }

    [Parameter]
    public EventCallback<Card> OnRemoveCardFromTurn { get; set; }
}


