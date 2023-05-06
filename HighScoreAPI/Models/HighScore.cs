namespace HighScoreAPI.Models;

public class HighScore
{
    public long Id { get; set; }
    public string Username { get; set; } = null!;
    public long Score { get; set; }

    public Project Project { get; set; } = null!;
}
