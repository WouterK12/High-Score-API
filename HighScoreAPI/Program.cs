using HighScoreAPI.Middleware;
using HighScoreAPI.DAL;
using HighScoreAPI.DAL.DataMappers;
using HighScoreAPI.Extensions;
using HighScoreAPI.Services;
using Microsoft.EntityFrameworkCore;
using ProfanityFilter.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithApiKeyHeader();

builder.Services.AddDbContextOptions<HighScoreContext>();
builder.Services.AddTransient<IHighScoreDataMapper, HighScoreDataMapper>();
builder.Services.AddTransient<IHighScoreService, HighScoreService>();
builder.Services.AddSingleton<IProfanityFilter>(_ => new ProfanityFilter.ProfanityFilter());

EnsureDatabaseIsCreated(builder);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.Run();

static void EnsureDatabaseIsCreated(WebApplicationBuilder builder)
{
    var dbContextOptions = builder.Services.BuildServiceProvider()
        .GetRequiredService<DbContextOptions<HighScoreContext>>();

    using var context = new HighScoreContext(dbContextOptions);
    context.Database.EnsureCreated();
}