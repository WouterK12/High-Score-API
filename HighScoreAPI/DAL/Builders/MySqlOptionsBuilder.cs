using Microsoft.EntityFrameworkCore;

namespace HighScoreAPI.DAL.Builders;

public static class MySqlOptionsBuilder
{
    public static DbContextOptionsBuilder BuildMySqlOptions(this DbContextOptionsBuilder builder, string connectionString)
    {
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 32));

        return builder.UseMySql(connectionString, serverVersion, (builder) =>
        {
            builder.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: new List<int> { });
        });
    }

    public static DbContextOptions<T> BuildContextOptions<T>(string connectionString) where T : DbContext
    {
        return (DbContextOptions<T>)new DbContextOptionsBuilder<T>()
            .BuildMySqlOptions(connectionString)
            .Options;
    }
}
