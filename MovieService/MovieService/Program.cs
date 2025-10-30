using Microsoft.OpenApi.Models;
using MovieService.Services;
using MovieService.Background;
using Serilog;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Load configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// 🔹 Configure Serilog (Logging)
builder.Host.UseSerilog((context, config) =>
    config.WriteTo.Console()
          .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
          .Enrich.FromLogContext()
          .MinimumLevel.Debug());

// 🔹 Add Controllers + Swagger
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

// 🔹 Add Health Checks
builder.Services.AddHealthChecks();

// 🔹 Add OpenTelemetry (Distributed Tracing)
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

// 🔹 Add Memory Cache
builder.Services.AddMemoryCache();

// 🔹 Register HTTP Client for TMDB
builder.Services.AddHttpClient("tmdb", client =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// 🔹 Register Dependencies
builder.Services.AddScoped<IMovieClient, TmdbMovieClient>();
builder.Services.AddScoped<IMovieService, MovieService.Services.MovieService>();

// 🔹 Background Worker (Cache warmup)
builder.Services.AddHostedService<CacheWarmupWorker>();

var app = builder.Build();

// 🔹 Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(); // Log each HTTP request
app.UseHttpsRedirection();
app.UseAuthorization();

// 🔹 Map Controllers + Health Check endpoint
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
