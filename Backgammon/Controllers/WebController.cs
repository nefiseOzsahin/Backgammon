using Microsoft.AspNetCore.Mvc;

namespace Backgammon.Controllers
{
    public class WebController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
