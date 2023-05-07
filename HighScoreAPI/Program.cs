using HighScoreAPI.Middleware;
using HighScoreAPI.DAL;
using HighScoreAPI.DAL.DataMappers;
using HighScoreAPI.Extensions;
using HighScoreAPI.Services;
using Microsoft.EntityFrameworkCore;
using ProfanityFilter.Interfaces;
using System.Text.Json.Serialization;
using HighScoreAPI.Middleware.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
                .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithApiKeyHeader();

builder.Services.AddDbContextOptions<DatabaseContext>();

builder.Services.AddTransient<IProjectDataMapper, ProjectDataMapper>();
builder.Services.AddTransient<IProjectService, ProjectService>();

builder.Services.AddTransient<IHighScoreDataMapper, HighScoreDataMapper>();
builder.Services.AddTransient<IHighScoreService, HighScoreService>();

builder.Services.AddSingleton<IProfanityFilter, ProfanityFilter.ProfanityFilter>();

builder.Services.AddSingleton<IRequestWriter, RequestWriter>();

EnsureDatabaseIsCreated(builder);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyMiddleware>();
app.UseMiddleware<EncryptionMiddleware>();

app.MapControllers();

app.Run();

static void EnsureDatabaseIsCreated(WebApplicationBuilder builder)
{
    var dbContextOptions = builder.Services.BuildServiceProvider()
        .GetRequiredService<DbContextOptions<DatabaseContext>>();

    using var context = new DatabaseContext(dbContextOptions);
    context.Database.EnsureCreated();
}
