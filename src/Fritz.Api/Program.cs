using Fritz.Api.Hubs;
using Fritz.Api.Middleware;
using Fritz.Api.Services;
using Fritz.Shared.DTOs;
using Fritz.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddSingleton<GameService>();
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    options.AddPolicy("AllowSignalR", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "https://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use API Key authentication
app.UseMiddleware<ApiKeyMiddleware>();

app.UseCors("AllowAll");

// API Endpoints
app.MapPost("/api/games/create", (CreateGameRequest request, GameService gameService) =>
{
    var game = gameService.CreateGame(request.PlayerName);
    
    return Results.Ok(new CreateGameResponse
    {
        GameCode = game.Code,
        GameId = game.Id,
        PlayerSymbol = PlayerSymbol.X.ToString()
    });
});

app.MapPost("/api/games/join", (JoinGameRequest request, GameService gameService) =>
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
});

app.MapGet("/api/games/{gameId}", (string gameId, GameService gameService) =>
{
    var game = gameService.GetGame(gameId);
    
    if (game == null)
    {
        return Results.NotFound();
    }
    
    return Results.Ok(new GameStateDto
    {
        GameId = game.Id,
        GameCode = game.Code,
        Board = game.Board,
        State = game.State.ToString(),
        CurrentTurn = game.CurrentTurn.ToString(),
        Winner = game.Winner?.ToString(),
        PlayerXFirstThreeInRow = game.FirstPlayerWithThree?.ToString()
    });
});

// Map SignalR hub
app.MapHub<GameHub>("/gamehub");

app.Run();
