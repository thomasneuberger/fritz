namespace Fritz.Shared.DTOs;

public class JoinGameRequest
{
    public string GameCode { get; set; } = string.Empty;
    public string? PlayerName { get; set; }
}
