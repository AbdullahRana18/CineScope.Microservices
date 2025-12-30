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

        // ---------------------------------------------------------
        // 1. Movies Index Page (Search & List)
        // ---------------------------------------------------------
        public async Task<IActionResult> Index(string searchQuery = "")
        {
            var client = _httpClientFactory.CreateClient();

            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Default trending search or user provided search query
            string apiUrl = string.IsNullOrEmpty(searchQuery)
                ? "https://localhost:7288/api/Movies/search?q=avengers"
                : $"https://localhost:7288/api/Movies/search?q={Uri.EscapeDataString(searchQuery)}";

            try
            {
                var movies = await client.GetFromJsonAsync<List<MovieDto>>(apiUrl);
                ViewData["SearchQuery"] = searchQuery;
                return View(movies ?? new List<MovieDto>());
            }
            catch (Exception)
            {
                // Handle cases where the backend API might be down
                return View(new List<MovieDto>());
            }
        }

        // ---------------------------------------------------------
        // 2. Movie Details Page
        // ---------------------------------------------------------
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("token");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string apiUrl = $"https://localhost:7288/api/Movies/{id}";

            try
            {
                var movie = await client.GetFromJsonAsync<MovieDto>(apiUrl);
                if (movie == null) return NotFound();
                return View(movie);
            }
            catch
            {
                return NotFound();
            }
        }

        // ---------------------------------------------------------
        // 3. Bulk Data Fetch (Parallelism Demo)
        // ---------------------------------------------------------

        // GET: Show the input form
        [HttpGet]
        public IActionResult BulkDemo()
        {
            return View(new List<MovieDto>());
        }

        // POST: Process the user input and fetch data
        [HttpPost]
        public async Task<IActionResult> BulkDemo(string movieIds)
        {
            var client = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("token");

            // Check if user is logged in
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 1. Validate Input
            if (string.IsNullOrWhiteSpace(movieIds))
            {
                ViewBag.Error = "Please enter at least one Movie ID.";
                return View(new List<MovieDto>());
            }

            // 2. Parse the string input (e.g., "550, 299536") into a List of Integers
            List<int> ids;
            try
            {
                ids = movieIds.Split(',')
                              .Select(id => int.Parse(id.Trim()))
                              .ToList();
            }
            catch
            {
                ViewBag.Error = "Invalid format! Please use numbers separated by commas (e.g., 550, 155).";
                return View(new List<MovieDto>());
            }

            // 3. Send Request to Backend
            // Note: The backend restricts this endpoint to 'Admin' role only.
            var response = await client.PostAsJsonAsync("https://localhost:7288/api/Movies/bulk", ids);

            if (!response.IsSuccessStatusCode)
            {
                // Return error if user does not have Admin role or API fails
                ViewBag.Error = "Access Denied or API Error! Ensure you are logged in as an Admin.";
                return View(new List<MovieDto>());
            }

            // 4. Return results to View
            var movies = await response.Content.ReadFromJsonAsync<List<MovieDto>>();
            return View(movies);
        }
    }
}