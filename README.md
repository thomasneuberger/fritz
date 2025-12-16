# Fritz - 5x5 Tic-Tac-Toe Game

Fritz is an exciting variant of Tic-Tac-Toe played on a 5x5 grid with a special rule that adds strategic depth to the classic game.

## Game Rules

- **Board Size**: 5x5 grid (instead of the traditional 3x3)
- **Win Condition**: Get 3 symbols in a row (horizontally, vertically, or diagonally)
- **Special Fritz Rule**: When a player achieves 3 in a row first, the opponent gets one final move:
  - If the opponent also achieves 3 in a row, **the opponent wins**
  - If the opponent fails to achieve 3 in a row, **the first player wins**

## Technology Stack

- **.NET 10** - Latest .NET framework
- **Blazor WebAssembly** - Client-side web framework
- **Progressive Web App (PWA)** - Installable on devices, works offline
- **MIT Licensed Dependencies** - All external dependencies use MIT license

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Running the Application

1. Clone the repository:
   ```bash
   git clone https://github.com/thomasneuberger/fritz.git
   cd fritz
   ```

2. Build the application:
   ```bash
   dotnet build Fritz.sln
   ```

3. Run the application:
   ```bash
   cd FritzApp
   dotnet run
   ```

4. Open your browser and navigate to the URL shown in the console (typically `http://localhost:5019`)

**Alternative**: You can open `Fritz.sln` in Visual Studio or any IDE that supports .NET solutions.

### Installing as PWA

When you open the app in a browser, you can install it as a Progressive Web App:

- **Desktop**: Look for the install icon in the address bar
- **Mobile**: Use the browser's "Add to Home Screen" option

## Features

- 🎮 **Two-player local gameplay** - Play on the same device
- 📱 **Mobile-optimized** - Designed for portrait mode on mobile devices
- 🎨 **Beautiful UI** - Modern gradient design with smooth animations
- 📲 **PWA Support** - Install and play offline
- ♿ **Touch-friendly** - Large, easy-to-tap buttons

## Project Structure

```
fritz/
├── Fritz.sln        # Solution file
├── FritzApp/        # Main application project
│   ├── Models/      # Game data models (Player, GameState, Cell)
│   ├── Services/    # Game logic (GameService)
│   ├── Pages/       # UI components (Game.razor)
│   ├── Layout/      # Layout components
│   └── wwwroot/     # Static files (CSS, icons, manifest)
├── README.md        # This file
└── LICENSE          # MIT License
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

Thomas Neuberger
