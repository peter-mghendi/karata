namespace Karata.Web.Models
{
    public partial class Game
    {
        public record GameDelta
        {
            public bool Reverse { get; set; } = false;
            public uint Skip { get; set; } = 1;
            public bool HasRequest { get; set; } = false;
            public bool HasSpecificRequest { get; set; } = false;
            public bool RemovesPreviousRequest { get; set; } = true;
            public uint Give { get; set; } = 0; // Cards given to the next player
            public uint Pick { get; set; } = 0; // Cards the current player should pick
        }
    }
}