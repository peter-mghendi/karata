using Karata.Kit.Trivia.Models;

namespace Karata.Web.Extensions;

public static class ModelExtensions
{
    public static string GetImageUrl(this TopicData topic)
    {
        return $"https://fakeimg.pl/345x300/282828/eae0d0/?retina=1&text={topic.Title}";
    }
}