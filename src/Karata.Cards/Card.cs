using System;
using static Karata.Cards.Card;

namespace Karata.Cards;

public partial record class Card(CardFace Face, CardSuit Suit) { }