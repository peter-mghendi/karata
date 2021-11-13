[![Build + Test](https://github.com/sixpeteunder/karata/actions/workflows/dotnet.yml/badge.svg)](https://github.com/sixpeteunder/karata/actions/workflows/dotnet.yml)

> This repository is not dormant! There is a lot of work going on over on the [gameplay](https://github.com/sixpeteunder/karata/tree/gameplay) branch.

> In case you're interested, most of the work is happening [here](https://github.com/sixpeteunder/karata/blob/gameplay/src/Karata.Web/Engines/KarataEngine.cs) (game logic), [here](https://github.com/sixpeteunder/karata/blob/gameplay/src/Karata.Web/Pages/Play.razor) (UI) and [here](https://github.com/sixpeteunder/karata/blob/gameplay/src/Karata.Web/Hubs/GameHub.cs) (real-time coordination).

# karata
Real-time Kenyan street poker over ASP.NET Core SignalR/websockets.

The game is currently playable but does not implement all of the game logic yet.

There is also a custom cards library [here](https://github.com/sixpeteunder/karata/tree/main/src/Karata.Cards) (with a complete test suite).

## Features
- [x] Real-time in-game chat.
- [x] Real-time gameplay.
- [ ] Game logic - 90% (Aces don't completely work yet!).
- [ ] New UI - [In progress](https://github.com/sixpeteunder/karata/commit/4d12942a51b67801c772f4fd27d6bc507e7cf0d4).
- [ ] Password-protected rooms - Trivial.
- [ ] Configurable rules - technically already possible via [IEngine](https://github.com/sixpeteunder/karata/blob/gameplay/src/Karata.Web/Engines/IEngine.cs).
- [ ] Resumable games - [some steps made](https://github.com/sixpeteunder/karata/blob/gameplay/src/Karata.Web/Models/Turn.cs).
- [ ] Player disconnection/reconnection handling - no progress, blocked by resumable games.
- [ ] Game replays - no progress.
- [ ] Friend system - no progress.
- [ ] Tournaments/Knockouts - no progress.
- [ ] Feed - on [this page](https://github.com/sixpeteunder/karata/blob/gameplay/src/Karata.Web/Pages/Index.razor), maybe?
