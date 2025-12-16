namespace Fritz.Shared.Models;

public enum GameState
{
    WaitingForPlayers = 0,
    PlayerXTurn = 1,
    PlayerOTurn = 2,
    PlayerXWon = 3,
    PlayerOWon = 4,
    Draw = 5,
    LastChance = 6  // Special state when one player got 3 in a row, other player has one more turn
}
