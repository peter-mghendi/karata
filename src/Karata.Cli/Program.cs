using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Karata.Cards;
using System.Text.Json;

// var deck1 = Deck.StandardDeck;
// var deck2 = Deck.StandardDeck;
// deck2.Shuffle();

// Console.WriteLine(deck1.Cards.SequenceEqual(deck2.Cards));
// Console.WriteLine(deck1.Cards.Aggregate(0, (a, c) => HashCode.Combine(a, c.GetHashCode())));
// Console.WriteLine(deck2.Cards.Aggregate(0, (a, c) => HashCode.Combine(a, c.GetHashCode())));

// StreamReader reader = new("cards.json");
// var jsonString = await reader.ReadToEndAsync();
// var cardList = JsonSerializer.Deserialize<List<Card>>(jsonString);
// Console.WriteLine(cardList[0].Name);

var deck = Deck.StandardDeck;
deck.Shuffle();
DisplayDeck(deck: deck);

static void DisplayDeck(Deck deck)
{
    var title = $"| {"Card",-20}| {"Rank",-5}|";

    Console.WriteLine($"{deck.Count} cards.\n");
    Console.WriteLine($"{title}\n{new string('-', title.Length)}");
    foreach (var card in deck) Console.WriteLine($"| {card.Name,-20}| {card.Rank,-5}|");
}
