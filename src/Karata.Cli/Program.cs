using System;
using Karata.Cards;
using Karata.Cards.Shufflers;

var deck = Deck.StandardDeck;
// deck.Shuffle(ShuffleAlgorithm.FisherYates);
// deck.Shuffle(ShuffleAlgorithm.OrderByRandom);
DisplayDeck(deck: deck);

void DisplayDeck(Deck deck)
{
    var cards = deck.Cards;
    var title = $"| {"Card",-20}| {"Rank",-5}|";

    Console.WriteLine($"{cards.Count} cards.\n");
    Console.WriteLine($"{title}\n{new string('-', title.Length)}");
    foreach (var card in cards) Console.WriteLine($"| {card.Name,-20}| {card.Rank,-5}|");
}
