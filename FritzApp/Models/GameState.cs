namespace FritzApp.Models;

public enum GameState
{
    InProgress,
    FirstRowAchieved,  // First player got 3 in a row, waiting for opponent's response
    GameOver
}
