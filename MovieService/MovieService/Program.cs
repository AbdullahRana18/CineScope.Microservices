using Microsoft.OpenApi.Models;
using MovieService.Services;
using MovieService.Background;
using Serilog;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Host.UseSerilog((context, config) =>
    config.WriteTo.Console()
          .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
          .Enrich.FromLogContext()
          .MinimumLevel.Debug());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MovieService API",
        Version = "v1",
        Description = "API for searching and fetching movies using TMDB."
    });
});

builder.Services.AddHealthChecks();

builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient("tmdb", client =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// ✅ FIXED — Correct AuthService URL
builder.Services.AddHttpClient("auth", client =>
{
    client.BaseAddress = new Uri("https://localhost:7016/api/Auth/");
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddScoped<IMovieClient, TmdbMovieClient>();
builder.Services.AddScoped<IMovieService, MovieService.Services.MovieService>();

builder.Services.AddHostedService<CacheWarmupWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
