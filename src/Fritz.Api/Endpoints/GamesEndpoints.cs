using Fritz.Api.Services;
using Fritz.Shared.DTOs;
using Fritz.Shared.Models;

namespace Fritz.Api.Endpoints;

public static class GamesEndpoints
{
    public static void MapGamesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/games");

        group.MapPost("/create", CreateGame);
        group.MapPost("/join", JoinGame);
        group.MapGet("/{gameId}", GetGame);
    }

    private static IResult CreateGame(CreateGameRequest request, GameService gameService)
    {
        var game = gameService.CreateGame(request.PlayerName);

        return Results.Ok(new CreateGameResponse
        {
            GameCode = game.Code,
            GameId = game.Id,
            PlayerSymbol = PlayerSymbol.X.ToString()
        });
    }

    private static IResult JoinGame(JoinGameRequest request, GameService gameService)
    {
        var game = gameService.JoinGame(request.GameCode, request.PlayerName);

        if (game == null)
        {
            return Results.NotFound(new { message = "Game not found or already started" });
        }

        return Results.Ok(new JoinGameResponse
        {
            GameId = game.Id,
            PlayerSymbol = PlayerSymbol.O.ToString()
        });
    }

    private static IResult GetGame(string gameId, GameService gameService)
    {
        var game = gameService.GetGame(gameId);

        if (game == null)
        {
            return Results.NotFound();
        }

        // Convert 2D array to jagged array for JSON serialization
        var board = new PlayerSymbol[5][];
        for (int i = 0; i < 5; i++)
        {
            board[i] = new PlayerSymbol[5];
            for (int j = 0; j < 5; j++)
            {
                board[i][j] = game.Board[i, j];
            }
        }

        return Results.Ok(new GameStateDto
        {
            GameId = game.Id,
            GameCode = game.Code,
            Board = board,
            State = game.State.ToString(),
            CurrentTurn = game.CurrentTurn.ToString(),
            Winner = game.Winner?.ToString(),
            PlayerXFirstThreeInRow = game.FirstPlayerWithThree?.ToString()
        });
    }
}
