using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace MovieWebUI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var client = _httpClientFactory.CreateClient("auth");

            var response = await client.PostAsJsonAsync("Auth/login", new
            {
                Username = username,
                PasswordHash = password
            });

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Invalid username or password";
                return View();
            }

            var json = await response.Content.ReadFromJsonAsync<TokenResponse>();

            // Call validate endpoint to get role
            var validateRes = await client.PostAsJsonAsync("Auth/validate", new { Token = json.Token });
            validateRes.EnsureSuccessStatusCode();
            var validateJson = await validateRes.Content.ReadFromJsonAsync<ValidateResponse>();

            // Save token, role, and username in session
            HttpContext.Session.SetString("token", json.Token);
            HttpContext.Session.SetString("role", validateJson.Role);
            HttpContext.Session.SetString("username", username);

            if (validateJson.Role == "Admin")
                return Redirect("/Admin/Dashboard");
            else
                return Redirect("/Movies/Index");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            var client = _httpClientFactory.CreateClient("auth");

            var response = await client.PostAsJsonAsync("Auth/register", new
            {
                Username = username,
                Email = email,
                PasswordHash = password
            });

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Registration failed";
                return View();
            }

            return RedirectToAction("Login");
        }

        public class TokenResponse
        {
            public string Token { get; set; }
        }

        public class ValidateResponse
        {
            public string Role { get; set; }
        }
    }
}
