using Fritz.Shared.DTOs;
using Microsoft.AspNetCore.SignalR.Client;

namespace Fritz.Client.Services;

public class GameHubService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly string _apiKey;
    
    public event Action<GameStateDto>? OnGameStateUpdated;
    public event Action<string>? OnPlayerJoined;
    public event Action<string>? OnInvalidMove;

    public GameHubService(Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _apiKey = configuration["ApiKey"] ?? "fritz-game-api-key-2024";
    }

    public async Task ConnectAsync(string hubUrl)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.Headers.Add("X-Api-Key", _apiKey);
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<GameStateDto>("GameStateUpdated", (gameState) =>
        {
            OnGameStateUpdated?.Invoke(gameState);
        });

        _hubConnection.On<string>("PlayerJoined", (playerSymbol) =>
        {
            OnPlayerJoined?.Invoke(playerSymbol);
        });

        _hubConnection.On<string>("InvalidMove", (message) =>
        {
            OnInvalidMove?.Invoke(message);
        });

        await _hubConnection.StartAsync();
    }

    public async Task JoinGameRoomAsync(string gameId, string playerSymbol)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.InvokeAsync("JoinGameRoom", gameId, playerSymbol);
        }
    }

    public async Task MakeMoveAsync(string gameId, string playerSymbol, MakeMoveRequest move)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.InvokeAsync("MakeMove", gameId, playerSymbol, move);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
