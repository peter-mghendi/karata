using Karata.Kit.Platform.Models;
using Karata.Kit.Trivia.Models;
using Karata.Trivia.Models;

namespace Karata.Trivia.Extensions;

public static class ResponseExtensions
{
    public static ChoiceData ToData(this Choice choice) => new(choice.Id, choice.Text, choice.IsCorrect);

    public static GameData ToData(this Game game) => new(
        game.Id,
        game.Topic.ToData(),
        [.. game.Players],
        game.Creator,
        game.CreatedAt
    );

    public static QuestionData ToData(this Question question) => new(
        question.Id,
        question.Text,
        [.. question.Choices.Select(c => c.ToData())]
    );

    public static ResponseData ToData(this Response response) => new(
        response.Id,
        response.TimeLeft,
        response.Score,
        response.Choice?.ToData(),
        response.User
    );

    public static RoundData ToData(this Round round) => new(
        round.Id,
        round.Index,
        round.Question.ToData(),
        [.. round.Responses.Select(r => r.ToData())]
    );

    public static TopicData ToData(this Topic topic) => new(topic.Id, topic.Title, topic.Description);
}