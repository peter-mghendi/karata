using Karata.Kit.Domain.Models;
using Karata.Server.Hubs.Clients;
using static Karata.Kit.Domain.Models.MessageType;

namespace Karata.Server.Services;

public sealed class ReplaySessionRunner(
    Guid roomId,
    IReadOnlyList<Turn> turns,
    IReplayerClient replayer,
    TimeSpan interval,
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
                    await replayer.MoveCardsFromDeckToHand(roomId, turn.Hand.Id, turn.CardsPicked);
                    break;
                case TurnType.Fail:
                    await replayer.SystemMessage(roomId, new SystemMessage { Text = turn.Metadata.Problem!.Message, Type = Error });
                    break;
                case TurnType.Play:
                    await replayer.MoveCardsFromHandToPile(roomId, turn.Hand.Id, turn.Delta!.Cards, false);

                    if (turn.ReclaimedPile) await replayer.ReclaimPile(roomId);
                    if (turn.CardsPicked.Count > 0) await replayer.MoveCardsFromDeckToHand(roomId, turn.Hand.Id, turn.CardsPicked);
                    if (turn.GameResult is not null) await replayer.EndGame(roomId, turn.GameResult.ToData());

                    await replayer.TurnCommitted(roomId, turn.GameSnapshot!);
                    break;
                case TurnType.Skip:
                    await replayer.SystemMessage(roomId, new SystemMessage { Text = $"{username} was skipped.", Type = Info });
                    break;
                case TurnType.Void:
                    await replayer.SystemMessage(roomId, new SystemMessage { Text = $"{username} voided their turn.", Type = Warning });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (index < count - 1) await Task.Delay(interval, cancellation);
        }
    }
}