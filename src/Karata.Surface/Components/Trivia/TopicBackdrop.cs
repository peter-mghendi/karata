using Karata.Kit.Trivia.Models;

namespace Karata.Surface.Components.Trivia;

public static class TopicBackdrop
{
    public static string For(TopicData topic)
    {
        var hash = Hash($"{topic.Id}");
        var hue = (int)(hash % 360);
        var shift = 25 + (int)(hash / 360 % 40);

        return $"background: linear-gradient(135deg, hsl({hue}, 55%, 42%), hsl({(hue + shift) % 360}, 60%, 32%));";
    }

    private static uint Hash(string value)
    {
        unchecked
        {
            var hash = 2166136261u;
            foreach (var c in value)
            {
                hash ^= c;
                hash *= 16777619u;
            }

            return hash;
        }
    }
}