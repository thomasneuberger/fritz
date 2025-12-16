using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fritz.Shared.DTOs;
using Microsoft.Extensions.Configuration;

namespace Fritz.Client.Services;

public class GameApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GameApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["ApiKey"] ?? "fritz-game-api-key-2024";
        
        var apiBaseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5000";
        _httpClient.BaseAddress = new Uri(apiBaseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
    }

    public async Task<CreateGameResponse?> CreateGameAsync(CreateGameRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/games/create", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CreateGameResponse>();
    }

    public async Task<JoinGameResponse?> JoinGameAsync(JoinGameRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/games/join", request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<JoinGameResponse>();
    }

    public async Task<GameStateDto?> GetGameStateAsync(string gameId)
    {
        return await _httpClient.GetFromJsonAsync<GameStateDto>($"/api/games/{gameId}");
    }
}
