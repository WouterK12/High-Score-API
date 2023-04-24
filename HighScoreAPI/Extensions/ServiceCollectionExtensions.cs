using HighScoreAPI.Middleware;
using HighScoreAPI.DAL.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace HighScoreAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerGenWithApiKeyHeader(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "High Score API", Version = "1" });
            c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme()
            {
                Name = HeaderNames.XAPIKey,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKeyScheme"
            });

            var key = new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                In = ParameterLocation.Header
            };
            var requirement = new OpenApiSecurityRequirement() { { key, new List<string>() } };
            c.AddSecurityRequirement(requirement);
        });

        return services;
    }

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
