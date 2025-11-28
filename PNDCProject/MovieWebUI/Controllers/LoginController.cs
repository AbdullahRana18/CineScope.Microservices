using Microsoft.AspNetCore.Mvc;

namespace MovieWebUI.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
