using Fritz.Shared.Models;

namespace Fritz.Shared.DTOs;

public class GameStateDto
{
    public string GameId { get; set; } = string.Empty;
    public string GameCode { get; set; } = string.Empty;
    public PlayerSymbol[][] Board { get; set; } = InitializeBoard();
    public string State { get; set; } = string.Empty;
    public string? CurrentTurn { get; set; }
    public string? Winner { get; set; }
    public string? PlayerXFirstThreeInRow { get; set; }  // For tracking the special last-chance rule

    private static PlayerSymbol[][] InitializeBoard()
    {
        var board = new PlayerSymbol[5][];
        for (int i = 0; i < 5; i++)
        {
            board[i] = new PlayerSymbol[5];
        }
        return board;
    }
}
