using Karata.Kit.Cards.Models;
using Karata.Kit.Trivia.Models;
using Karata.Kit.Trivia.Models.Response;
using Karata.Trivia.Data;
using Karata.Trivia.Models;

namespace Karata.Trivia.Extensions;

public static class ResponseExtensions
{
    public static ChoiceResponse AsResponse(this Choice choice) => new(choice.Id, choice.Text, choice.IsCorrect);

    public static GameResponse AsResponse(this Game game) => new(
        game.Id,
        game.Identifier,
        game.Topic.AsResponse(),
        game.PlayerOne.AsUserData(),
        game.PlayerTwo.AsUserData()
    );

    public static NotificationResponse AsResponse(this Notification notification)
    {
        return new NotificationResponse(
            notification.Id,
            notification.Action,
            notification.SentAt,
            notification.ReadAt,
            notification.Game.AsResponse()
        );
    }

    public static NotificationSubscriptionData AsResponse(this NotificationSubscription subscription)
    {
        return new NotificationSubscriptionData(
            NotificationSubscriptionId: subscription.Id,
            UserId: subscription.User.Id,
            Url: subscription.Url,
            P256dh: subscription.P256dh,
            Auth: subscription.Auth
        );
    }

    public static QuestionResponse AsResponse(this Question question) => new(
        question.Id,
        question.Text,
        question.Choices.Select(c => c.AsResponse()).ToList()
    );

    public static ResponseResponse AsResponse(this Response response) => new(
        response.Id,
        response.TimeLeft,
        response.Points,
        response.Choice?.AsResponse(),
        response.User.AsUserData()
    );

    public static RoundResponse AsResponse(this Round round) => new(
        round.Id,
        round.Index,
        round.Question.AsResponse(),
        round.Responses.Select(r => r.AsResponse()).ToList()
    );

    public static TopicResponse AsResponse(this Topic topic) => new(topic.Id, topic.Title, topic.Description);

    public static UserData AsUserData(this User user) => new() { Id = user.Id, Username = user.Username };
}