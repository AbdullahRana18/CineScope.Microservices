using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MovieWebUI.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // ✅ Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // ✅ Users List
        public async Task<IActionResult> UsersList()
        {
            var client = _httpClientFactory.CreateClient("auth");

            var token = HttpContext.Session.GetString("token");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            // ✅ FIXED URL (backend ke mutabiq)
            var users = await client.GetFromJsonAsync<List<UserDto>>("Auth/users");

            return View(users ?? new List<UserDto>());
        }

        // ✅ Edit User
        public async Task<IActionResult> EditUser(int id)
        {
            var client = _httpClientFactory.CreateClient("auth");
            var token = HttpContext.Session.GetString("token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var user = await client.GetFromJsonAsync<UserDto>($"Auth/users/{id}");
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(UserDto model)
        {
            var client = _httpClientFactory.CreateClient("auth");

            var token = HttpContext.Session.GetString("token");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            await client.PutAsJsonAsync($"Auth/users/{model.Id}", model);

            return RedirectToAction("UsersList");
        }

        // ✅ Delete User
        public async Task<IActionResult> DeleteUser(int id)
        {
            var client = _httpClientFactory.CreateClient("auth");
            var token = HttpContext.Session.GetString("token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await client.DeleteAsync($"Auth/users/{id}");
            return RedirectToAction("UsersList");
        }
    }

    // ✅ DTO
    public class UserDto
    {
        public int Id { get; set; } 
        public string Username { get; set; } 
        public string Email { get; set; } 
        public string Role { get; set; } 
    }
}
