using Microsoft.OpenApi.Models;
using MovieService.Services;
using MovieService.Background;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MovieService API", Version = "v1" });
});

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient("tmdb", client =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    client.Timeout = TimeSpan.FromSeconds(10);
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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
