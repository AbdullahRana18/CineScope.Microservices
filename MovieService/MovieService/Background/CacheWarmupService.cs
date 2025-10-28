using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MovieService.Services;

namespace MovieService.Background
{
    public class CacheWarmupWorker : BackgroundService
    {
        private readonly ILogger<CacheWarmupWorker> _log;
        private readonly IServiceScopeFactory _scopeFactory;

        public CacheWarmupWorker(IServiceScopeFactory scopeFactory, ILogger<CacheWarmupWorker> log)
        {
            _scopeFactory = scopeFactory;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _log.LogInformation("CacheWarmupWorker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var movieService = scope.ServiceProvider.GetRequiredService<IMovieService>();

                        var queries = new[] { "star wars", "batman", "avengers" };
                        foreach (var q in queries)
                        {
                            await movieService.SearchMoviesAsync(q, stoppingToken);
                        }
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Warmup error");
                }

                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }

            _log.LogInformation("CacheWarmupWorker stopping");
        }
    }
}
