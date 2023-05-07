namespace HighScoreAPI.Models;

public class Project
{
    public string Name { get; set; } = null!;
    public string AesKeyBase64 { get; set; } = null!;

    public IList<HighScore> HighScores { get; set; } = new List<HighScore>();
}
