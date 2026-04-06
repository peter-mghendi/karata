using Karata.Cards;

namespace Karata.Kit.Domain.Models;

public sealed record TurnResolution(
    int CurrentTurn,
    UserData Player,
    Card? CurrentRequest,
    uint PendingPick,
    bool IsCardless,
    bool IsLastCard
);