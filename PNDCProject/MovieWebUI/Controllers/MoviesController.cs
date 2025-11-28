using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MovieWebUI.Models;

namespace MovieWebUI.Controllers
{
    public class MoviesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MoviesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Movies Index page
        public async Task<IActionResult> Index(string searchQuery = "")
        {
            var client = _httpClientFactory.CreateClient();

            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Default trending search
            string apiUrl = string.IsNullOrEmpty(searchQuery)
                ? "https://localhost:7288/api/Movies/search?q=avengers"
                : $"https://localhost:7288/api/Movies/search?q={Uri.EscapeDataString(searchQuery)}";

            var movies = await client.GetFromJsonAsync<List<MovieDto>>(apiUrl);

            ViewData["SearchQuery"] = searchQuery;

            return View(movies ?? new List<MovieDto>());
        }
    }
}
