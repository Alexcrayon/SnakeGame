# Multiplayer Snake

An online multiplayer Snake game written in C#. One server runs the game. Players connect to it with a desktop client and play in the same world.

Built by Lucas Jones and Alex Cao for CS 3500 at the University of Utah (Fall 2022).

<!-- Add a screenshot or short GIF of gameplay here. -->

## Features

- **Online multiplayer over TCP.** Many players can connect to one server.
- **Authoritative server.** The server owns the game state. Clients send only movement commands, and the server decides the result.
- **Fixed-timestep game loop.** By default, the server updates the world every 17 ms (about 60 frames per second).
- **Power-ups.** Each power-up adds 1 point and makes the snake 50 units longer.
- **Camera that follows the player.** The client centers the view on your snake.
- **Explosion animation.** The client plays an animation when a snake dies.
- **Survival mode.** An optional server mode. See [Server settings](#server-settings).

## Repository layout

| Folder | Contents |
|---|---|
| `PS7/NetworkController` | TCP networking library. It uses asynchronous sockets with callback-based event loops. |
| `PS7/NetworkTests` | Unit tests for the networking library (provided by the course). |
| `PS7/NetworkTestHelper` | Helper code for the networking tests. |
| `PS8Skeleton/NetworkController` | The copy of the networking library that the game uses. |
| `PS8Skeleton/Model` | Game objects: `Snake`, `Wall`, `Powerup`, `Explosion`, `ClientWorld`, and `ServerWorld`. |
| `PS8Skeleton/GameController` | Client controller. It connects to the server, reads server messages, and updates the client world. |
| `PS8Skeleton/SnakeClient` | .NET MAUI client (the view). It shows the connection screen, reads keyboard input, and draws the world in `WorldPanel`. |
| `PS8Skeleton/Server` | Server entry point (`SnakeServer.cs`) and `settings.xml`. |
| `PS8Skeleton/ServerController` | Server game logic: movement (`SnakeController`), collisions (`CollisionController`), and settings (`GameSettings`). |
| `PS8Skeleton/Vector2D` | 2D vector math. |

## Architecture

### Client

The client uses the Model-View-Controller (MVC) pattern:

- **Model:** `ClientWorld` holds all objects in the world. Each object stores its own values, such as whether it is alive.
- **View:** `SnakeClient`. `MainPage` gives the `GameController` to `WorldPanel`. `WorldPanel` reads the world with `GetWorld()` and draws it.
- **Controller:** `GameController` holds the world, the player ID, and the player name. It is the only part that changes the world. It sends movement commands to the server, and it uses events to tell `MainPage` about errors and new worlds.

### Server

`SnakeServer` runs the game loop. Each frame, it uses `ServerController` to update the model in this order:

1. Remove the power-ups that were collected in the last frame.
2. Move each snake and check it for collisions.
3. Respawn the dead snakes whose respawn timer is done.
4. Send every snake and power-up to every connected client.

The server keeps its own `ServerWorld`, separate from the client's `ClientWorld`. This separation keeps server logic out of the client.

Socket callbacks run on different threads from the game loop. The server uses locks to keep the world and the client list consistent between these threads.

## Network protocol

Each message is a JSON object followed by a newline (`\n`). The receiver splits incoming data on newlines. It keeps an incomplete message in its buffer until the rest of the message arrives.

1. The client connects to the server on TCP port `11000`. The client sends the player's name.
2. The server sends the player ID, the world size, and each wall.
3. When the server receives the name, it creates a snake for that player.
4. Each frame, the server sends every snake and power-up.
5. The client sends a movement command when the player presses a key:

   ```json
   {"moving":"up"}
   ```

   The value is `up`, `down`, `left`, `right`, or `none`.

## Requirements

- .NET 6 SDK
- Visual Studio 2022 with the .NET Multi-platform App UI (MAUI) workload
- Windows or macOS for the client

.NET 6 reached end of support in November 2024. To build with a newer SDK, you may need to change the target framework in each `.csproj` file.

## Run the game

1. Open `PS8Skeleton/SnakeGame.sln` in Visual Studio.
2. Build the solution.
3. Start the `Server` project. The console shows `Server now running...`, and the server listens on port `11000`.
4. Start the `SnakeClient` project.
5. Enter the server address (for example, `localhost`) and a player name of 16 characters or fewer.
6. Click **Connect**. Your snake appears in the world.
7. Use **W**, **A**, **S**, and **D** to steer.

To add more players, start more clients and connect them to the same server.

## Server settings

The server reads `settings.xml` from its working directory at startup. The build copies this file to the output folder.

| Setting | Default | Meaning |
|---|---|---|
| `MSPerFrame` | `17` | Milliseconds per frame. |
| `RespawnRate` | `150` | Frames before a dead snake respawns. |
| `UniverseSize` | `2000` | Width and height of the square world. |
| `SurvivalMode` | `false` | Turns survival mode on or off (see below). |
| `Walls` | 4 walls | List of wall segments. Each wall has an `ID`, a start point `p1`, and an end point `p2`. |
| `FramesPerShot` | `80` | Read at startup, but not used by this game. |

**Survival mode:** Snakes grow without limit, and no power-ups appear. Your score is the number of seconds your snake stays alive.

## Design notes

- **Explosions are separate world objects.** At first, the snake drawer also drew the explosion. That failed when two or more snakes died at the same time. Now the controller adds an `Explosion` object when a snake dies, so each explosion has its own position and frame count.
- **Snakes grow out at the start.** A new snake begins short and grows to its full length.
- **Respawn time is short.** A dead snake stays visible long enough for the player to see what happened, then respawns.
- **Disconnected snakes stay briefly.** When a client disconnects, its snake stays on screen for a short time before the server removes it.
- **The speed-boost power-up was removed.** It needed a new `type` field in the power-up JSON. The course-provided client could not read that field, so we removed the feature.

## Known issues

- A part of a snake's body sometimes draws as a square. This happens when a point in the body disappears. We saw it four times in a week of testing and could not reproduce it.


## Credits

- **Authors:** Lucas Jones and Alex Cao
- **Course:** CS 3500, University of Utah. The course provided the project specification, the protocol, and the networking unit tests (written by Daniel Kopta).
- **Explosion art:** Sogomn, on [OpenGameArt](https://opengameart.org/content/explosion-3)
