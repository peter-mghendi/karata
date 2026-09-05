using Karata.Kit.Platform.Models;

namespace Karata.Surface.Extensions;

public static class UserDataExtensions
{
    extension(UserData user)
    {
        // TODO [HACK] Until I get round to building a proper avatar system
        public string Avatar => $"https://api.dicebear.com/10.x/glyphs/svg?seed={user.Username}";
    }
}