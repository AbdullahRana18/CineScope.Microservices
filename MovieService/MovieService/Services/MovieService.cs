using MovieService.Models;
using Microsoft.Extensions.Caching.Memory;

namespace MovieService.Services
{
    public interface IMovieService
    {
        Task<IEnumerable<MovieDto>> SearchMoviesAsync(string q, CancellationToken ct);
        Task<IEnumerable<MovieDto>> GetMultipleByIdsParallelAsync(IEnumerable<int> ids, CancellationToken ct);
    }

    public class MovieService : IMovieService
    {
        private readonly IMovieClient _client;
        private readonly IMemoryCache _cache;
        private readonly ILogger<MovieService> _log;
        private readonly TimeSpan _cacheDuration;

        public MovieService(IMovieClient client, IMemoryCache cache, IConfiguration cfg, ILogger<MovieService> log)
        {
            _client = client;
            _cache = cache;
            _log = log;
            _cacheDuration = TimeSpan.FromMinutes(int.TryParse(cfg["Cache:DefaultDurationMinutes"], out var m) ? m : 10);
        }

        public async Task<IEnumerable<MovieDto>> SearchMoviesAsync(string q, CancellationToken ct)
        {
            var key = $"search:{q}";
            if (_cache.TryGetValue(key, out List<MovieDto> cached)) return cached;

            var tmdb = await _client.SearchAsync(q, ct);
            var list = tmdb?.Results.Select(m => new MovieDto
            {
                Id = m.Id,
                Title = m.Title,
                Overview = m.Overview,
                PosterPath = m.PosterPath,
                ReleaseDate = m.ReleaseDate
            }).ToList() ?? new List<MovieDto>();

            _cache.Set(key, list, _cacheDuration);
            return list;
        }

        // Demonstrates concurrency: fetch many movie details in parallel with cancellation support
        public async Task<IEnumerable<MovieDto>> GetMultipleByIdsParallelAsync(IEnumerable<int> ids, CancellationToken ct)
        {
            var idList = ids.Distinct().ToList();
            var tasks = idList.Select(async id =>
            {
                var cacheKey = $"movie:{id}";
                if (_cache.TryGetValue(cacheKey, out MovieDto mv)) return mv;

                // Each GetMovieByIdAsync is async and non-blocking
                var tm = await _client.GetMovieByIdAsync(id, ct);
                if (tm == null) return null;

                var dto = new MovieDto
                {
                    Id = tm.Id,
                    Title = tm.Title,
                    Overview = tm.Overview,
                    PosterPath = tm.PosterPath,
                    ReleaseDate = tm.ReleaseDate
                };
                _cache.Set(cacheKey, dto, _cacheDuration);
                return dto;
            });

            // Task.WhenAll runs these concurrently
            var results = await Task.WhenAll(tasks);
            return results.Where(r => r != null)!.Select(r => r!);
        }
    }
}
