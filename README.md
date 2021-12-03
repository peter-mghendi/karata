[![Build + Test](https://github.com/sixpeteunder/karata/actions/workflows/dotnet.yml/badge.svg)](https://github.com/sixpeteunder/karata/actions/workflows/dotnet.yml)

> STATUS: Currently making [connection handling](https://github.com/sixpeteunder/karata/issues/8) more robust.

# karata

Real-time Kenyan street poker over ASP.NET Core SignalR/websockets.

The game is currently playable and implements all of the game logic.

There is also a custom cards library [here](https://github.com/sixpeteunder/karata/tree/main/src/Karata.Cards) (with a complete test suite).

## Features
- [x] Real-time in-game chat.
- [x] Real-time gameplay.
- [x] Game logic.
- [x] Password-protected rooms.
- [ ] Configurable "fines" (off by default) - Trivial.
- [ ] New UI - [In progress](https://github.com/sixpeteunder/karata/commit/4d12942a51b67801c772f4fd27d6bc507e7cf0d4), blocked by my knowledge of CSS 😂.
- [ ] Player disconnection/reconnection handling - being tracked in [#8](https://github.com/sixpeteunder/karata/issues/8).
- [ ] Resumable games - blocked by connection handling.
- [ ] Configurable rules - technically already possible via [IEngine](https://github.com/sixpeteunder/karata/blob/main/src/Karata.Web/Engines/IEngine.cs).
- [ ] Game replays - no progress.
- [ ] Friend system - no progress.
- [ ] Tournaments/Knockouts - no progress.
- [ ] Feed - on [this page](https://github.com/sixpeteunder/karata/blob/main/src/Karata.Web/Pages/Index.razor), maybe?

## Rules

> The rules are automatically applied to games and you do not need to actively think about them (unless fines are enabled!)
> This is mostly included for reference and troubleshooting the game's behaviour.
> I should probably add these to an in-game "rules" page.

None of the sources I consulted could agree on a canonical set of rules (as they should) so I implemented some sensible defaults:

I'm looking into a way to make the rules configurable without complicating the codebase too much.

### Basics
- The game can only start and end with a non-special card (any card other than those described below).
- Players may choose to enable a one or two card "fine" for invalid moves.
- Fines are off by default and enabled on a per-game basis.
- The winner is the first player to discard all of theiir cards while on "last card" status.
- A player cannot enter "last card" status while in possesion of an Ace, "Bomb", Jack or King.
- A card sequence that would usually cause the player to play again, e.g two Kings or "jumping" everyone, is counted as its own turn.

### Aces
- Ace of Spades equals two regular Aces.
- One Ace can be used to request a suit.
- Two Aces (or equivalent) can be used to request a specific card.
- Aces can be used to block "bomb" cards.
- Aces can play anywhere.
- Any number of Aces is valid, but three or four aces have have no special effects.
- Two aces can request a specific Joker but one Ace can not request a Joker.

### "Bombs" - Twos, Threes and Jokers
- Two, three and joker cards cause the next player to pick two, three or five cards respectively.
- Two and three cards can be countered by jokers or "bomb" cards of the same face or suit.
- Jokers can only be countered by jokers or blocked by a single Ace.
- Two and three cards can only play on top cards of the same face or suit.
- Jokers can play anywhere.
- Anything can play on top of jokers.
- Picking is not cumulative. Only the top card's value need be picked.
- Picking cannot be "jumped" or "kicked back".

### "Jumps" - Jacks
- A Jack played will "jump" the next player (two Jacks played in succession will jump two players, etc).
- A Jack must be played on top of a card of the same face(Jack) or suit.
- Jumping cannot be blocked, e.g. by another Jack placed by a "jumped player".

### "Questions" - Queens and Eights
- Queen and Eight cards are "Question" cards which require an "Answer".
- A Queen or Eight must be played on top of a card of the same face or suit.
- Valid answer cards are any cards of the same face or suit (including other questions.
- Everything is a valid answer card.

### "Kickbacks" - Kings
- A King will cause the direction of the game to reverse.
- A King must be played on top of a card of the same face(King) or suit.
- An even number of Kings played at once will cause the current player to play again.
- A single King played in a two-person game will have no effect.
