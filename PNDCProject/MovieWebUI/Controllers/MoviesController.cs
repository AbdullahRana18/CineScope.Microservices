using Microsoft.AspNetCore.Mvc;

namespace MovieWebUI.Controllers
{
    public class MoviesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
