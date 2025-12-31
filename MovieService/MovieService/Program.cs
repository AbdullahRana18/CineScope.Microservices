using Microsoft.OpenApi.Models;
using MovieService.Services;
using MovieService.Background;
using Serilog;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// ✅ Load settings from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// ✅ Setup Serilog for logging
builder.Host.UseSerilog((context, config) =>
    config.WriteTo.Console()
          .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
          .Enrich.FromLogContext()
          .MinimumLevel.Debug());

// ✅ Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✅ Swagger setup
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MovieService API",
        Version = "v1",
        Description = "API for searching and fetching movies using TMDB."
    });
});

//  Health check
builder.Services.AddHealthChecks();

//  OpenTelemetry tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

//  In-memory cache
builder.Services.AddMemoryCache();

//  HTTP clients
builder.Services.AddHttpClient("tmdb", client =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient("auth", client =>
{
    client.BaseAddress = new Uri("https://localhost:7016/api/Auth/");
    client.Timeout = TimeSpan.FromSeconds(5);
});

//  Register custom services
builder.Services.AddScoped<IMovieClient, TmdbMovieClient>();
builder.Services.AddScoped<IMovieService, MovieService.Services.MovieService>();

//  Background worker
builder.Services.AddHostedService<CacheWarmupWorker>();

//  CORS policy for frontend and other services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://localhost:7160", "https://localhost:7288") // React frontend & MovieService frontend
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

//  Swagger for development
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

//  Logging
app.UseSerilogRequestLogging();

//  HTTPS
app.UseHttpsRedirection();

// Apply CORS
app.UseCors("AllowAll");

//  Authorization middleware
app.UseAuthorization();

//  Map controllers
app.MapControllers();

//  Health endpoint
app.MapHealthChecks("/health");

app.Run();
