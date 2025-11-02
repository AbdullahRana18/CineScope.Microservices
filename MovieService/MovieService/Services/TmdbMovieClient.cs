using System.Net.Http.Json;
using System.Net.Http.Headers;
using MovieService.Models;
using Microsoft.Extensions.Configuration;

namespace MovieService.Services
{
    public class TmdbMovieClient : IMovieClient
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly string _apiKey;
        private readonly ILogger<TmdbMovieClient> _log;

        public TmdbMovieClient(IHttpClientFactory httpFactory, IConfiguration cfg, ILogger<TmdbMovieClient> log)
        {
            _httpFactory = httpFactory;
            _apiKey = cfg["TMDB:ApiKey"] ?? throw new ArgumentNullException("TMDB:ApiKey missing");
            _log = log;
        }

        // Search movies from TMDB
        public async Task<TmdbSearchResponse?> SearchAsync(string query, CancellationToken ct)
        {
            var client = _httpFactory.CreateClient("tmdb");

            // Add the API key as a Bearer token
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            var url = $"search/movie?query={Uri.EscapeDataString(query)}&page=1";

            // Send request to TMDB
            var res = await client.GetAsync(url, ct);
            if (!res.IsSuccessStatusCode)
            {
                _log.LogWarning("TMDB search failed: {Status}", res.StatusCode);
                return null;
            }

            // Convert JSON response to C# model
            var dto = await res.Content.ReadFromJsonAsync<TmdbSearchResponse>(cancellationToken: ct);
            return dto;
        }

        // Get movie details by ID
        public async Task<TmdbMovie?> GetMovieByIdAsync(int id, CancellationToken ct)
        {
            var client = _httpFactory.CreateClient("tmdb");

            // Add the API key as a Bearer token
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            var url = $"movie/{id}";

            // Send request to TMDB
            var res = await client.GetAsync(url, ct);
            if (!res.IsSuccessStatusCode)
            {
                _log.LogWarning("TMDB get movie {Id} failed: {Status}", id, res.StatusCode);
                return null;
            }

            // Convert JSON response to C# model
            var dto = await res.Content.ReadFromJsonAsync<TmdbMovie>(cancellationToken: ct);
            return dto;
        }
    }
}
