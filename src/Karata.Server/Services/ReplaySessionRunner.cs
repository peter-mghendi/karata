using Karata.Kit.Domain.Models;
using Karata.Server.Hubs.Clients;
using Karata.Server.Support;
using static Karata.Kit.Domain.Models.MessageType;

namespace Karata.Server.Services;

public sealed class ReplaySessionRunner(
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
                    await replayer.MoveCardsFromDeckToHand(turn.Hand.Id, turn.CardsPicked);
                    break;
                case TurnType.Fail:
                    await replayer.ReceiveSystemMessage(new SystemMessage { Text = turn.Metadata.Problem!.Message, Type = Error });
                    break;
                case TurnType.Play:
                    await replayer.SetCurrentRequest(turn.GameSnapshot!.Request);
                    await replayer.MoveCardsFromHandToPile(turn.Hand.Id, turn.Delta!.Cards, false);

                    if (turn.ReclaimedPile) await replayer.ReclaimPile();
                    if (turn.CardsPicked.Count > 0) await replayer.MoveCardsFromDeckToHand(turn.Hand.Id, turn.CardsPicked);
                    if (turn.GameResult is not null) await replayer.ReceiveSystemMessage(Messages.GameOver(turn.GameResult.ToData()));
                    if (turn.IsCardless) await replayer.ReceiveSystemMessage(Messages.Cardless(turn.Hand.Player));
                    if (turn.IsLastCard) await replayer.ReceiveSystemMessage(Messages.LastCard(turn.Hand.Player));
        
                    await replayer.UpdatePick(turn.GameSnapshot.Give);
                    await replayer.UpdateTurn(turn.GameSnapshot.CurrentTurn);
                    break;
                case TurnType.Skip:
                    await replayer.ReceiveSystemMessage(new SystemMessage { Text = $"{username} was skipped.", Type = Info });
                    break;
                case TurnType.Void:
                    await replayer.ReceiveSystemMessage(new SystemMessage { Text = $"{username} voided their turn.", Type = Warning });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (index < count - 1) await Task.Delay(interval, cancellation);
        }
    }
}