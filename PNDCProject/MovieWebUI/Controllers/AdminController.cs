using Microsoft.AspNetCore.Mvc;

namespace MovieWebUI.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            
            return View();
        }
    }
}
