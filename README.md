[![Build + Test](https://github.com/sixpeteunder/karata/actions/workflows/dotnet.yml/badge.svg)](https://github.com/sixpeteunder/karata/actions/workflows/dotnet.yml)

# karata

> Karata (cards) is a Swahili word that refers to both the Kenyan game of cards and the cards used to play it.

Real-time Kenyan street poker over ASP.NET Core SignalR/websockets.

The game is currently playable and implements all game logic.

There is also a custom cards library [here](https://github.com/sixpeteunder/karata/tree/main/src/Karata.Cards) (with a complete test suite).

## Features
- [x] Real-time in-game chat.
- [x] Real-time gameplay.
- [x] Game logic.
- [x] Activity Feed
- [x] Password-protected rooms.
- [x] Player disconnection/reconnection handling.
- [x] Resumable games.
- [ ] Configurable rules.
- [ ] Game replays.
- [ ] Friend system.
- [ ] Tournaments/Knockouts.
- [ ] Fines for illegal moves.

## Rules

> The rules are automatically applied to games, you do not need to actively think about them (unless fines are enabled!)
> This is mostly included for reference and troubleshooting the game's behaviour.
> I should probably add these to an in-game "rules" page.

None of the sources I consulted could agree on a canonical set of rules (as they should) so I implemented some sensible defaults:

### Basics
- The game can only start and end with a non-special card (any card other than those described below).
- Players may choose to enable a one or two card "fine" for invalid moves.
- Fines are off by default and enabled on a per-game basis.
- The winner is the first player to discard all of their cards while on "last card" status.
- A player cannot enter "last card" status while in possession of an Ace, "Bomb", Jack or King.
- A card sequence that would usually cause the player to play again, e.g. two Kings or "jumping" everyone, is counted as its own turn.

### Aces

![Ace of Spades](https://github.com/sixpeteunder/karata/blob/main/src/Karata.Client/wwwroot/img/cards/AceSpades.svg)

- Ace of Spades equals two regular Aces.
- One Ace can be used to request a suit.
- Two Aces (or equivalent) can be used to request a specific card.
- Aces can be used to block "bomb" cards.
- Aces can play anywhere.
- Any number of Aces is valid, but three or four aces have no special effects.
- Two aces can request a specific Joker but one Ace can not request a Joker.

### "Bombs" - Twos, Threes and Jokers

![Two of Spades](https://github.com/sixpeteunder/karata/blob/main/src/Karata.Client/wwwroot/img/cards/TwoSpades.svg)
![Three of Spades](https://github.com/sixpeteunder/karata/blob/main/src/Karata.Client/wwwroot/img/cards/ThreeSpades.svg)
![Black Joker](https://github.com/sixpeteunder/karata/blob/main/src/Karata.Client/wwwroot/img/cards/BlackJoker.svg)

- Two, three and joker cards cause the next player to pick two, three or five cards respectively.
- Two and three cards can be countered by jokers or "bomb" cards of the same face or suit.
- Jokers can only be countered by jokers or blocked by a single Ace.
- Two and three cards can only play on top cards of the same face or suit.
- Jokers can play anywhere.
- Anything can play on top of jokers.
- Picking is not cumulative. Only the top card's value need be picked.
- Picking cannot be "jumped" or "kicked back".

### "Jumps" - Jacks

![Jack of Hearts](https://github.com/sixpeteunder/karata/blob/main/src/Karata.Client/wwwroot/img/cards/JackHearts.svg)

- A Jack played will "jump" the next player (two Jacks played in succession will jump two players, etc.).
- A Jack must be played on top of a card of the same face(Jack) or suit.
- Jumping cannot be blocked, e.g. by another Jack placed by a "jumped player".

### "Questions" - Queens and Eights

![Queen of Hearts](https://github.com/sixpeteunder/karata/blob/main/src/Karata.Client/wwwroot/img/cards/QueenHearts.svg)
![Eight of Hearts](https://github.com/sixpeteunder/karata/blob/main/src/Karata.Client/wwwroot/img/cards/EightHearts.svg)

- Queen and Eight cards are "Question" cards which require an "Answer".
- A Queen or Eight must be played on top of a card of the same face or suit.
- Valid answer cards are any cards of the same face or suit (including other questions).
- Every rank of card (Ace to King) is a valid answer card.

### "Kickbacks" - Kings

![Kind of Hearts](https://github.com/sixpeteunder/karata/blob/main/src/Karata.Client/wwwroot/img/cards/KingHearts.svg)

- A King will cause the direction of the game to reverse.
- A King must be played on top of a card of the same face(King) or suit.
- An even number of Kings played at once will cause the current player to play again.
- A single King played in a two-person game will have no effect.
