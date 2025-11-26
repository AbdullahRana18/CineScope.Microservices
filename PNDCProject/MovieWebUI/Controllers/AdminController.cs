using Microsoft.AspNetCore.Mvc;

namespace MovieWebUI.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            // Admin login ke baad ye page dikhega
            return View();
        }
    }
}
