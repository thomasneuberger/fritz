namespace FritzApp.Models;

public class Cell
{
    public int Row { get; set; }
    public int Column { get; set; }
    public Player Player { get; set; } = Player.None;
}
