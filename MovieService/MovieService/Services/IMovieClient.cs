using MovieService.Models;

namespace MovieService.Services
{
    public interface IMovieClient
    {
        Task<TmdbSearchResponse?> SearchAsync(string query, CancellationToken ct);
        Task<TmdbMovie?> GetMovieByIdAsync(int id, CancellationToken ct);
    }
}
