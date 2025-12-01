using Microsoft.AspNetCore.Mvc;
using MovieService.Services;
using MovieService.Models;
using System.Net.Http.Json;

namespace MovieService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _svc;
        private readonly ILogger<MoviesController> _log;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _cfg;

        public MoviesController(IMovieService svc, ILogger<MoviesController> log, IHttpClientFactory httpFactory, IConfiguration cfg)
        {
            _svc = svc;
            _log = log;
            _httpFactory = httpFactory;
            _cfg = cfg;
        }

        
        // Token validation helper
        
        private async Task<(bool Valid, string Role)> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return (false, "");

            try
            {
                var client = _httpFactory.CreateClient("auth");
                var res = await client.PostAsJsonAsync("validate", new { Token = token });

                if (!res.IsSuccessStatusCode)
                {
                    _log.LogWarning("Token validation failed with status: {Status}", res.StatusCode);
                    return (false, "");
                }

                var data = await res.Content.ReadFromJsonAsync<TokenValidationResult>();
                return (data?.Valid ?? false, data?.Role ?? "");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error validating token with AuthService");
                return (false, "");
            }
        }

        
        // Search movies by query
        // GET: api/Movies/search?q=keyword
        
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var (valid, role) = await ValidateTokenAsync(token);

            if (!valid)
                return Unauthorized("Invalid or missing token.");

            if (role != "User" && role != "Admin")
                return Forbid();

            var res = await _svc.SearchMoviesAsync(q, ct);
            return Ok(res);
        }

        
        // Get movie details by ID
        // GET: api/Movies/{id}
      
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var (valid, role) = await ValidateTokenAsync(token);

            if (!valid)
                return Unauthorized("Invalid or missing token.");

            if (role != "User" && role != "Admin")
                return Forbid();

            var movie = await _svc.GetMovieByIdAsync(id, ct);
            if (movie == null)
                return NotFound($"Movie with ID {id} not found.");

            return Ok(movie);
        }

        
        // Fetch multiple movies by IDs
        // POST: api/Movies/bulk
      
        [HttpPost("bulk")]
        public async Task<IActionResult> Bulk([FromBody] List<int> ids, CancellationToken ct)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var (valid, role) = await ValidateTokenAsync(token);

            if (!valid)
                return Unauthorized("Invalid or missing token.");

            if (role != "Admin")
                return Forbid();

            if (ids == null || ids.Count == 0)
                return BadRequest("Please provide at least one movie ID.");

            var res = await _svc.GetMultipleByIdsParallelAsync(ids.ToArray(), ct);
            return Ok(res);
        }

        
        // Token validation response model
       
        public class TokenValidationResult
        {
            public bool Valid { get; set; }
            public string Role { get; set; } = "";
        }
    }
}
