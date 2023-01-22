using HighScoreServer.DAL.Builders;
using Microsoft.EntityFrameworkCore;

namespace HighScoreServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbContextOptions<T>(this IServiceCollection services) where T : DbContext
    {
        string? mysqlHostname = Environment.GetEnvironmentVariable("MYSQL_HOSTNAME");
        string? mysqlUser = Environment.GetEnvironmentVariable("MYSQL_USER");
        string? mysqlPassword = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");
        string? mysqlDatabaseName = Environment.GetEnvironmentVariable("MYSQL_DATABASE_NAME");

        string connectionString = string.Format("Server={0};Database={1};User={2};Password={3}", mysqlHostname, mysqlDatabaseName, mysqlUser, mysqlPassword);

        var dbContextOptions = MySqlOptionsBuilder.BuildContextOptions<T>(connectionString);

        services.AddSingleton(dbContextOptions);

        return services;
    }
}
