using HighScoreAPI.DTOs;
using HighScoreAPI.Models;

namespace HighScoreAPI.Extensions;

public static class HighScoreExtensions
{
    public static HighScore ToHighScore(this HighScoreDTO highScoreDTO)
    {
        return new HighScore() { Username = highScoreDTO.Username, Score = highScoreDTO.Score };
    }

    public static HighScoreDTO ToDTO(this HighScore highScore)
    {
        return new HighScoreDTO(highScore.Username, highScore.Score);
    }
}
