using Karata.Cards;

namespace Karata.Web.Models
{
    public partial class Game
    {
        public record GameDelta
        {
            public bool Reverse { get; set; } = false;
            public uint Skip { get; set; } = 1;
            // public Card Request { get; set; } = null;

            // Cards given to the next player
            // public uint Give { get; set; } = 0;

            // Cards the current player should pick
            public uint Pick { get; set; } = 0;
        }

        public void ApplyGameDelta(GameDelta delta)
        {
            // if (delta.Request is not null) CurrentRequest = delta.Request;
            if (delta.Reverse) IsForward = !IsForward;
            Pick = delta.Pick;
            Skip(delta.Skip);
        }

        public void Skip(uint turns)
        {
            var lastIndex = Players.Count - 1;
            for (uint i = 0; i < turns; i++)
            {
                if (IsForward)
                    CurrentTurn = CurrentTurn == lastIndex ? 0 : CurrentTurn + 1;
                else
                    CurrentTurn = CurrentTurn == 0 ? lastIndex : CurrentTurn - 1;
            }
        }
    }
}