namespace Fritz.Shared.Models;

public record Position(int Row, int Col)
{
    public bool IsValid() => Row >= 0 && Row < 5 && Col >= 0 && Col < 5;
}
