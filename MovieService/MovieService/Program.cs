using Microsoft.OpenApi.Models;
using MovieService.Services;
using MovieService.Background;
using Serilog;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Load settings from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Setup Serilog for logging to console and file
builder.Host.UseSerilog((context, config) =>
    config.WriteTo.Console()
          .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
          .Enrich.FromLogContext()
          .MinimumLevel.Debug());

// Add core services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger setup for API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MovieService API",
        Version = "v1",
        Description = "API for searching and fetching movies using TMDB."
    });
});

// Health check endpoint
builder.Services.AddHealthChecks();

// Add tracing for monitoring requests
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

// Add in-memory cache
builder.Services.AddMemoryCache();

// HTTP client for TMDB API
builder.Services.AddHttpClient("tmdb", client =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// HTTP client for AuthService API
builder.Services.AddHttpClient("auth", client =>
{
    client.BaseAddress = new Uri("https://localhost:7016/api/Auth/");
    client.Timeout = TimeSpan.FromSeconds(5);
});

// Register custom services
builder.Services.AddScoped<IMovieClient, TmdbMovieClient>();
builder.Services.AddScoped<IMovieService, MovieService.Services.MovieService>();

// Background worker to preload movie cache
builder.Services.AddHostedService<CacheWarmupWorker>();

var app = builder.Build();

// Enable Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", context =>
    {
    context.Response.Redirect("/swagger/index.html");
    return Task.CompletedTask;
    });
}

// Log every request
app.UseSerilogRequestLogging();

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Authorization middleware
app.UseAuthorization();

// Map API controllers
app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

app.Run();
