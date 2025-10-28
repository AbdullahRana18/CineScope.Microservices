using Microsoft.AspNetCore.Mvc;
using MovieService.Services;
using MovieService.Models;

namespace MovieService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _svc;
        private readonly ILogger<MoviesController> _log;

        public MoviesController(IMovieService svc, ILogger<MoviesController> log)
        {
            _svc = svc;
            _log = log;
        }

        // GET api/movies/search?q=batman
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest("q required");
            var res = await _svc.SearchMoviesAsync(q, ct);
            return Ok(res);
        }

        // POST api/movies/bulk
        // body: { "ids": [123, 456, 789] }
        [HttpPost("bulk")]
        public async Task<IActionResult> Bulk([FromBody] IdsRequest req, CancellationToken ct)
        {
            if (req?.Ids == null || req.Ids.Length == 0) return BadRequest("ids required");
            var res = await _svc.GetMultipleByIdsParallelAsync(req.Ids, ct);
            return Ok(res);
        }

        // simple model for POST
        public class IdsRequest { public int[] Ids { get; set; } = Array.Empty<int>(); }
    }
}
