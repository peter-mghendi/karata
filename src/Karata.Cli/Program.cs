using System.Collections.Concurrent;
using System.Text.Json;
using Karata.Cards;
using Karata.Cards.Extensions;
using static System.Console;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;

DisplayDeck(deck: Deck.StandardDeck);

var deck1 = Deck.StandardDeck;
var deck2 = Deck.StandardDeck;
deck2.Shuffle();

WriteLine(deck1.SequenceEqual(deck2));
WriteLine(deck1.Aggregate(0, (a, c) => HashCode.Combine(a, c.GetHashCode())));
WriteLine(deck2.Aggregate(0, (a, c) => HashCode.Combine(a, c.GetHashCode())));

StreamReader reader = new("cards.json");
var cardList = JsonSerializer.Deserialize<List<Card>>(await reader.ReadToEndAsync());
if (cardList is not null) WriteLine(cardList[0].GetName());

var dictionary = new ConcurrentDictionary<string, string>();
AddToDictionary(dictionary, "key1", "value1");
AddToDictionary(dictionary, "key2", "value2");
WriteLine(dictionary.Count);

WriteLine(nameof(I.Action));

static void DisplayDeck(Deck deck)
{
    var title = $"| {"Card",-20}| {"Rank",-5}|";

    WriteLine($"{deck.Count} cards.\n\n{title}\n{new string('-', title.Length)}");
    foreach (var card in deck) WriteLine($"| {card.GetName(),-20}| {card.GetRank(),-5}|");
}

static void AddToDictionary(ConcurrentDictionary<string, string> dict, string key, string value)
{
    if (dict.TryAdd(key, value)) WriteLine($"Added {key} to dictionary.");
    else WriteLine($"Failed to add {key} to dictionary.");
}

interface I
{
    public void Action(string parameter);
}