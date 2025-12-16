# Fritz Game

A 5x5 Tic-Tac-Toe game with a unique twist! This is a modern Progressive Web App (PWA) built with .NET 10.

## Game Rules

Unlike traditional Tic-Tac-Toe:
- **5x5 Grid**: The board is larger than the traditional 3x3
- **Get 3 in a Row**: The goal is still to get 3 symbols in a row (horizontally, vertically, or diagonally)
- **Last Chance Rule**: When one player gets 3 in a row, the opponent gets **one more turn** to also get 3 in a row
  - If the opponent succeeds, they win!
  - If the opponent fails, the first player wins

## Technology Stack

### Backend
- **ASP.NET Web API** (.NET 10)
- **SignalR** for real-time communication
- **Docker** containerization support
- **API Key Authentication** for security

### Frontend
- **Blazor WebAssembly** Standalone
- **Progressive Web App (PWA)** - installable on devices
- **SignalR Client** for real-time game updates
- **Mobile-first design** optimized for portrait mode

### Shared
- **Common models and DTOs** shared between client and server

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/) (optional, for containerized deployment)

### Running Locally

#### 1. Run the Backend API

```bash
cd src/Fritz.Api
dotnet run
```

The API will start on `https://localhost:7169` (HTTPS) and `http://localhost:5121` (HTTP)

#### 2. Client Configuration

The client is pre-configured in `src/Fritz.Client/wwwroot/appsettings.json`:

```json
{
  "ApiBaseUrl": "https://localhost:7169",
  "ApiKey": "fritz-game-api-key-2024"
}
```

#### 3. Run the Blazor Client

```bash
cd src/Fritz.Client
dotnet run --launch-profile https
```

The client will be available at `https://localhost:7290` (HTTPS) and `http://localhost:5010` (HTTP)

### Running with Docker

Build and run the API in Docker:

```bash
cd src/Fritz.Api
docker build -t fritz-api -f Dockerfile ../..
docker run -p 5000:8080 -e ApiKey=fritz-game-api-key-2024 fritz-api
```

## Project Structure

```
fritz/
├── src/
│   ├── Fritz.Api/          # Backend API
│   │   ├── Hubs/           # SignalR hubs
│   │   ├── Services/       # Game logic services
│   │   ├── Models/         # Domain models
│   │   ├── Middleware/     # API key authentication
│   │   └── Dockerfile      # Docker configuration
│   ├── Fritz.Client/       # Blazor WebAssembly PWA
│   │   ├── Pages/          # Razor pages
│   │   ├── Services/       # API and Hub services
│   │   └── wwwroot/        # Static assets & PWA files
│   └── Fritz.Shared/       # Shared models and DTOs
│       ├── Models/         # Game models (enums, etc.)
│       └── DTOs/           # Data transfer objects
└── Fritz.sln               # Solution file
```

## How to Play

1. **Start a New Game**
   - Open the app and click "Start New Game"
   - You'll receive a 6-character game code
   - Share this code with your opponent

2. **Join an Existing Game**
   - Open the app and click "Join Existing Game"
   - Enter the game code provided by your friend
   - Click "Join Game"

3. **Play**
   - Players take turns placing their symbol (X or O)
   - Try to get 3 in a row (horizontal, vertical, or diagonal)
   - When you get 3 in a row, your opponent gets one final chance!

## API Endpoints

### REST Endpoints
- `POST /api/games/create` - Create a new game
- `POST /api/games/join` - Join an existing game
- `GET /api/games/{gameId}` - Get game state

### SignalR Hub
- `/gamehub` - Real-time game communication
  - `JoinGameRoom(gameId, playerSymbol)` - Join a game room
  - `MakeMove(gameId, playerSymbol, move)` - Make a move
  - Events: `GameStateUpdated`, `PlayerJoined`, `InvalidMove`

## Configuration

### API Configuration (`src/Fritz.Api/appsettings.json`)
```json
{
  "ApiKey": "fritz-game-api-key-2024"
}
```

### Client Configuration (`src/Fritz.Client/wwwroot/appsettings.json`)
```json
{
  "ApiBaseUrl": "http://localhost:5000",
  "ApiKey": "fritz-game-api-key-2024"
}
```

## License

MIT License - see [LICENSE](LICENSE) file for details

## Author

Thomas Neuberger
