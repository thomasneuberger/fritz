using FritzApp.Models;

namespace FritzApp.Services;

public class GameService
{
    private const int BoardSize = 5;
    private const int WinLength = 3;

    public Cell[,] Board { get; private set; } = new Cell[BoardSize, BoardSize];
    public Player CurrentPlayer { get; private set; } = Player.X;
    public GameState State { get; private set; } = GameState.InProgress;
    public Player? Winner { get; private set; }
    public Player? FirstRowPlayer { get; private set; }  // Player who achieved first row of 3
    public string StatusMessage { get; private set; } = "Player X's turn";

    public GameService()
    {
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                Board[row, col] = new Cell { Row = row, Column = col, Player = Player.None };
            }
        }
    }

    public void NewGame()
    {
        InitializeBoard();
        CurrentPlayer = Player.X;
        State = GameState.InProgress;
        Winner = null;
        FirstRowPlayer = null;
        StatusMessage = "Player X's turn";
    }

    public bool MakeMove(int row, int col)
    {
        if (State == GameState.GameOver)
            return false;

        if (Board[row, col].Player != Player.None)
            return false;

        Board[row, col].Player = CurrentPlayer;

        // Check if current player has achieved 3 in a row
        if (HasThreeInRow(CurrentPlayer))
        {
            if (State == GameState.InProgress)
            {
                // First player achieved 3 in a row
                State = GameState.FirstRowAchieved;
                FirstRowPlayer = CurrentPlayer;
                SwitchPlayer();
                StatusMessage = $"Player {CurrentPlayer} has one more move to tie!";
                return true;
            }
            else if (State == GameState.FirstRowAchieved)
            {
                // Opponent also achieved 3 in a row - opponent wins!
                State = GameState.GameOver;
                Winner = CurrentPlayer;
                StatusMessage = $"Player {CurrentPlayer} wins by achieving 3 in a row after opponent!";
                return true;
            }
        }

        // If we're in FirstRowAchieved state and opponent didn't achieve 3 in a row, first player wins
        if (State == GameState.FirstRowAchieved)
        {
            State = GameState.GameOver;
            Winner = FirstRowPlayer;
            StatusMessage = $"Player {Winner} wins!";
            return true;
        }

        // Check for draw (board full)
        if (IsBoardFull())
        {
            State = GameState.GameOver;
            StatusMessage = "It's a draw!";
            return true;
        }

        // Continue game
        SwitchPlayer();
        StatusMessage = $"Player {CurrentPlayer}'s turn";
        return true;
    }

    private void SwitchPlayer()
    {
        CurrentPlayer = CurrentPlayer == Player.X ? Player.O : Player.X;
    }

    private bool HasThreeInRow(Player player)
    {
        // Check horizontal
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col <= BoardSize - WinLength; col++)
            {
                if (Board[row, col].Player == player &&
                    Board[row, col + 1].Player == player &&
                    Board[row, col + 2].Player == player)
                {
                    return true;
                }
            }
        }

        // Check vertical
        for (int col = 0; col < BoardSize; col++)
        {
            for (int row = 0; row <= BoardSize - WinLength; row++)
            {
                if (Board[row, col].Player == player &&
                    Board[row + 1, col].Player == player &&
                    Board[row + 2, col].Player == player)
                {
                    return true;
                }
            }
        }

        // Check diagonal (top-left to bottom-right)
        for (int row = 0; row <= BoardSize - WinLength; row++)
        {
            for (int col = 0; col <= BoardSize - WinLength; col++)
            {
                if (Board[row, col].Player == player &&
                    Board[row + 1, col + 1].Player == player &&
                    Board[row + 2, col + 2].Player == player)
                {
                    return true;
                }
            }
        }

        // Check diagonal (top-right to bottom-left)
        for (int row = 0; row <= BoardSize - WinLength; row++)
        {
            for (int col = WinLength - 1; col < BoardSize; col++)
            {
                if (Board[row, col].Player == player &&
                    Board[row + 1, col - 1].Player == player &&
                    Board[row + 2, col - 2].Player == player)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsBoardFull()
    {
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                if (Board[row, col].Player == Player.None)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
