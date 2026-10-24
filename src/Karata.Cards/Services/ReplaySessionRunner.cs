using Karata.Cards.Hubs.Clients;
using Karata.Kit.Cards.Models;
using static Karata.Kit.Cards.Models.MessageType;

namespace Karata.Cards.Services;

public sealed class ReplaySessionRunner(
    ReplayRequest request,
    IReadOnlyList<Turn> turns,
    IReplayerClient replayer,
    CancellationToken cancellation
)
{
    public async Task RunAsync()
    {
        var count = turns.Count;
        
        foreach (var (index, turn) in turns.Index())
        {
            cancellation.ThrowIfCancellationRequested();

            var username = turn.Hand.Player.Username;
            switch (turn.Type)
            {
                case TurnType.Deal:
                    await replayer.MoveCardsFromDeckToHand(request.RoomId, turn.Hand.Id, turn.CardsPicked);
                    break;
                case TurnType.Fail:
                    await replayer.SystemMessage(request.RoomId, new SystemMessage { Text = turn.Metadata.Problem!.Message, Type = Error });
                    break;
                case TurnType.Play:
                    await replayer.MoveCardsFromHandToPile(request.RoomId, turn.Hand.Id, turn.Delta!.Cards, false);

                    if (turn.ReclaimedPile) await replayer.ReclaimPile(request.RoomId);
                    if (turn.CardsPicked.Count > 0) await replayer.MoveCardsFromDeckToHand(request.RoomId, turn.Hand.Id, turn.CardsPicked);
                    if (turn.GameResult is not null) await replayer.EndGame(request.RoomId, turn.GameResult.ToData());

                    await replayer.TurnCommitted(request.RoomId, turn.GameSnapshot!);
                    break;
                case TurnType.Skip:
                    await replayer.SystemMessage(request.RoomId, new SystemMessage { Text = $"{username} was skipped.", Type = Info });
                    break;
                case TurnType.Void:
                    await replayer.SystemMessage(request.RoomId, new SystemMessage { Text = $"{username} voided their turn.", Type = Warning });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (index < count - 1) await Task.Delay(request.Interval, cancellation);
        }
    }
}