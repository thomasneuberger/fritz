using Fritz.Shared.Models;

namespace Fritz.Api.Models;

public class Game
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Code { get; set; } = string.Empty;
    public PlayerSymbol[,] Board { get; set; } = new PlayerSymbol[5, 5];
    public GameState State { get; set; } = GameState.WaitingForPlayers;
    public PlayerSymbol CurrentTurn { get; set; } = PlayerSymbol.X;
    public PlayerSymbol? Winner { get; set; }
    public string? PlayerXName { get; set; }
    public string? PlayerOName { get; set; }
    public string? PlayerXConnectionId { get; set; }
    public string? PlayerOConnectionId { get; set; }
    public PlayerSymbol? FirstPlayerWithThree { get; set; }  // For tracking the special last-chance rule
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
