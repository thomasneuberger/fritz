using Fritz.Shared.DTOs;
using Microsoft.AspNetCore.SignalR.Client;

namespace Fritz.Client.Services;

public class GameHubService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    public event Action<GameStateDto>? OnGameStateUpdated;
    public event Action<string>? OnPlayerJoined;
    public event Action<string>? OnInvalidMove;

    public async Task ConnectAsync(string hubUrl)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
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
