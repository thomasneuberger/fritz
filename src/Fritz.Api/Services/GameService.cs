using Fritz.Api.Models;
using Fritz.Shared.Models;
using System.Collections.Concurrent;

namespace Fritz.Api.Services;

public class GameService
{
    private readonly ConcurrentDictionary<string, Game> _games = new();
    private readonly ConcurrentDictionary<string, string> _gameCodes = new();
    private readonly Random _random = new();

    public Game CreateGame(string? playerName)
    {
        var game = new Game
        {
            Code = GenerateGameCode(),
            PlayerXName = playerName,
            State = GameState.WaitingForPlayers
        };

        _games[game.Id] = game;
        _gameCodes[game.Code] = game.Id;

        return game;
    }

    public Game? JoinGame(string gameCode, string? playerName)
    {
        if (!_gameCodes.TryGetValue(gameCode, out var gameId))
            return null;

        if (!_games.TryGetValue(gameId, out var game))
            return null;

        if (game.State != GameState.WaitingForPlayers)
            return null;

        game.PlayerOName = playerName;
        game.State = GameState.PlayerXTurn;

        return game;
    }

    public Game? GetGame(string gameId)
    {
        _games.TryGetValue(gameId, out var game);
        return game;
    }

    public Game? GetGameByCode(string gameCode)
    {
        if (_gameCodes.TryGetValue(gameCode, out var gameId))
        {
            return GetGame(gameId);
        }
        return null;
    }

    public bool MakeMove(string gameId, PlayerSymbol player, Position position)
    {
        if (!_games.TryGetValue(gameId, out var game))
            return false;

        // Validate position
        if (!position.IsValid())
            return false;

        // Check if cell is already occupied
        if (game.Board[position.Row, position.Col] != PlayerSymbol.None)
            return false;

        // Check if it's the player's turn
        if (game.State == GameState.PlayerXTurn && player != PlayerSymbol.X)
            return false;
        if (game.State == GameState.PlayerOTurn && player != PlayerSymbol.O)
            return false;
        if (game.State == GameState.LastChance && player == game.FirstPlayerWithThree)
            return false;

        // Make the move
        game.Board[position.Row, position.Col] = player;

        // Check for win
        if (CheckWin(game, player))
        {
            if (game.State == GameState.LastChance)
            {
                // The second player also got 3 in a row - second player wins!
                game.Winner = player;
                game.State = player == PlayerSymbol.X ? GameState.PlayerXWon : GameState.PlayerOWon;
            }
            else
            {
                // First player to get 3 in a row - give opponent one more chance
                game.FirstPlayerWithThree = player;
                game.State = GameState.LastChance;
                game.CurrentTurn = player == PlayerSymbol.X ? PlayerSymbol.O : PlayerSymbol.X;
            }
        }
        else if (game.State == GameState.LastChance)
        {
            // Last chance used, first player wins
            game.Winner = game.FirstPlayerWithThree;
            game.State = game.FirstPlayerWithThree == PlayerSymbol.X ? GameState.PlayerXWon : GameState.PlayerOWon;
        }
        else if (IsBoardFull(game))
        {
            // Board is full and no winner - draw
            game.State = GameState.Draw;
        }
        else
        {
            // Continue game - switch turn
            game.CurrentTurn = player == PlayerSymbol.X ? PlayerSymbol.O : PlayerSymbol.X;
            game.State = game.CurrentTurn == PlayerSymbol.X ? GameState.PlayerXTurn : GameState.PlayerOTurn;
        }

        return true;
    }

    private bool CheckWin(Game game, PlayerSymbol player)
    {
        // Check rows
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col <= 2; col++)
            {
                if (game.Board[row, col] == player &&
                    game.Board[row, col + 1] == player &&
                    game.Board[row, col + 2] == player)
                    return true;
            }
        }

        // Check columns
        for (int col = 0; col < 5; col++)
        {
            for (int row = 0; row <= 2; row++)
            {
                if (game.Board[row, col] == player &&
                    game.Board[row + 1, col] == player &&
                    game.Board[row + 2, col] == player)
                    return true;
            }
        }

        // Check diagonals (top-left to bottom-right)
        for (int row = 0; row <= 2; row++)
        {
            for (int col = 0; col <= 2; col++)
            {
                if (game.Board[row, col] == player &&
                    game.Board[row + 1, col + 1] == player &&
                    game.Board[row + 2, col + 2] == player)
                    return true;
            }
        }

        // Check diagonals (top-right to bottom-left)
        for (int row = 0; row <= 2; row++)
        {
            for (int col = 2; col < 5; col++)
            {
                if (game.Board[row, col] == player &&
                    game.Board[row + 1, col - 1] == player &&
                    game.Board[row + 2, col - 2] == player)
                    return true;
            }
        }

        return false;
    }

    private bool IsBoardFull(Game game)
    {
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                if (game.Board[row, col] == PlayerSymbol.None)
                    return false;
            }
        }
        return true;
    }

    private string GenerateGameCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string code;
        do
        {
            code = new string(Enumerable.Range(0, 6)
                .Select(_ => chars[_random.Next(chars.Length)])
                .ToArray());
        } while (_gameCodes.ContainsKey(code));

        return code;
    }

    public void UpdatePlayerConnection(string gameId, PlayerSymbol player, string connectionId)
    {
        if (_games.TryGetValue(gameId, out var game))
        {
            if (player == PlayerSymbol.X)
                game.PlayerXConnectionId = connectionId;
            else if (player == PlayerSymbol.O)
                game.PlayerOConnectionId = connectionId;
        }
    }
}
