using Fritz.Api.Services;
using Fritz.Shared.DTOs;
using Fritz.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Fritz.Api.Hubs;

public class GameHub : Hub
{
    private readonly GameService _gameService;

    public GameHub(GameService gameService)
    {
        _gameService = gameService;
    }

    public async Task JoinGameRoom(string gameId, string playerSymbol)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        
        // Update player connection ID
        var symbol = Enum.Parse<PlayerSymbol>(playerSymbol);
        _gameService.UpdatePlayerConnection(gameId, symbol, Context.ConnectionId);
        
        var game = _gameService.GetGame(gameId);
        if (game != null)
        {
            await Clients.Group(gameId).SendAsync("PlayerJoined", playerSymbol);
            await Clients.Group(gameId).SendAsync("GameStateUpdated", MapGameToDto(game));
        }
    }

    public async Task MakeMove(string gameId, string playerSymbol, MakeMoveRequest move)
    {
        var symbol = Enum.Parse<PlayerSymbol>(playerSymbol);
        var position = new Position(move.Row, move.Col);
        
        var success = _gameService.MakeMove(gameId, symbol, position);
        
        if (success)
        {
            var game = _gameService.GetGame(gameId);
            if (game != null)
            {
                await Clients.Group(gameId).SendAsync("GameStateUpdated", MapGameToDto(game));
            }
        }
        else
        {
            await Clients.Caller.SendAsync("InvalidMove", "Invalid move");
        }
    }

    private GameStateDto MapGameToDto(Models.Game game)
    {
        return new GameStateDto
        {
            GameId = game.Id,
            GameCode = game.Code,
            Board = game.Board,
            State = game.State.ToString(),
            CurrentTurn = game.CurrentTurn.ToString(),
            Winner = game.Winner?.ToString(),
            PlayerXFirstThreeInRow = game.FirstPlayerWithThree?.ToString()
        };
    }
}
