using System.Collections.Concurrent;
using Karata.Cards;
using Karata.Cards.Extensions;
using Karata.Server.Models;
using Karata.Shared.Models;
using static System.Console;
using static Karata.Cards.Card.CardColor;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;
using static Karata.Shared.Models.CardRequestLevel;

// DisplayDeck(deck: Deck.StandardDeck);
//
// var deck1 = Deck.StandardDeck;
// var deck2 = Deck.StandardDeck;
// deck2.Shuffle();
//
// WriteLine(deck1.SequenceEqual(deck2));
// WriteLine(deck1.Aggregate(0, (a, c) => HashCode.Combine(a, c.GetHashCode())));
// WriteLine(deck2.Aggregate(0, (a, c) => HashCode.Combine(a, c.GetHashCode())));
//
// StreamReader reader = new("cards.json");
// var cardList = JsonSerializer.Deserialize<List<Card>>(await reader.ReadToEndAsync());
// if (cardList is not null) WriteLine(cardList[0].GetName());
//
// var dictionary = new Conc    urrentDictionary<string, string>();
// AddToDictionary(dictionary, "key1", "value1");
// AddToDictionary(dictionary, "key2", "value2");
// WriteLine(dictionary.Count);
//
// WriteLine(nameof(I.Action));


// Test cases
var testCases = new List<(List<Card>, Card?, int)>
{
    ([Ace.Of(Spades), Two.Of(Hearts), Ace.Of(Hearts)], Eight.Of(Diamonds), 1),
    ([Three.Of(Clubs), Ace.Of(Clubs), Ace.Of(Spades)], Nine.Of(Spades), 0),
    ([Ace.Of(Diamonds), Ace.Of(Clubs)], None.Of(Hearts), 2),
    ([Five.Of(Spades), Seven.Of(Hearts)], null, 1),
    ([Red.ColoredJoker(), Black.ColoredJoker()], null, 1),
};

foreach (var (cards, currentRequest, pick) in testCases)
{
    var game = new Game { Request = currentRequest, Pick = (uint)pick };
    var delta1 = new GameDelta();
    var delta2 = new GameDelta();

    TestBlock1(cards, game, delta1);
    TestBlock2(cards, game, delta2);

    Console.WriteLine($"Test case with {cards.Count} cards, CurrentRequest={currentRequest?.ToString() ?? "null"}, Pick={pick}");
    Console.WriteLine($"Block 1: RemoveRequestLevels = {delta1.RemoveRequestLevels}, RequestLevel = {delta1.RequestLevel}");
    Console.WriteLine($"Block 2: RemoveRequestLevels = {delta2.RemoveRequestLevels}, RequestLevel = {delta2.RequestLevel}");
    Console.WriteLine($"Match: {delta1.RemoveRequestLevels == delta2.RemoveRequestLevels && delta1.RequestLevel == delta2.RequestLevel}");
    Console.WriteLine("----------");
}

return;

static void DisplayDeck(Deck deck)
{
    var title = $"| {"Card",-20}| {"Rank",-5}|";

    WriteLine($"{deck.Count} cards.\n\n{title}\n{new string('-', title.Length)}");
    foreach (var card in deck) WriteLine($"| {card.GetName(),-20}| {card.GetRank(),-5}|");
}

static void AddToDictionary(ConcurrentDictionary<string, string> dict, string key, string value)
{
    WriteLine(dict.TryAdd(key, value) ? $"Added {key} to dictionary." : $"Failed to add {key} to dictionary.");
}

static void TestBlock1(List<Card> cards, Game game, GameDelta delta)
{
    var aces = cards.Sum(card => card.GetAceValue());
    delta = delta with { RemoveRequestLevels = (uint)Math.Min(aces, (long)game.RequestLevel) };
    aces -= (long)game.RequestLevel;
    if (game.Pick > 0) --aces;
    if (aces > 0) delta = delta with { RequestLevel = aces > 1 ? CardRequest : SuitRequest };
}

static void TestBlock2(List<Card> cards, Game game, GameDelta delta)
{
    var totalAces = cards.Sum(card => card.GetAceValue());
    var remainingAces = totalAces - (long)game.RequestLevel;
    delta = delta with { RemoveRequestLevels = (uint)Math.Min(totalAces, (long)game.RequestLevel) };
    if (game.Pick > 0) --remainingAces;
    if (remainingAces > 0) delta = delta with { RequestLevel = remainingAces > 1 ? CardRequest : SuitRequest };
}

internal interface I
{
    public void Action(string parameter);
}