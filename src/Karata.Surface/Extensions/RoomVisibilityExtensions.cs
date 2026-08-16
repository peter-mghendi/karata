using Karata.Kit.Cards.Models;

namespace Karata.Surface.Extensions;

public static class RoomVisibilityExtensions
{
    extension(RoomVisibility visibility)
    {
        public string Icon => visibility switch
        {
            RoomVisibility.Public => MudBlazor.Icons.Material.Rounded.Public,
            RoomVisibility.Unlisted => MudBlazor.Icons.Material.Rounded.Link,
            RoomVisibility.InviteOnly => MudBlazor.Icons.Material.Rounded.Lock,
            _ => throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null)
        };
    }
}