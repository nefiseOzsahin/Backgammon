using Microsoft.AspNetCore.Mvc;

namespace Backgammon.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {          
            return View();
        }
    }
}
