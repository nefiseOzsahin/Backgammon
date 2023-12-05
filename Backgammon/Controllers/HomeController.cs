using Backgammon.Business;
using Backgammon.Context;
using Backgammon.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backgammon.Controllers
{
    public class HomeController : Controller
    {
        private readonly BackgammonContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly UserService _userService;


        public HomeController(BackgammonContext context, UserManager<AppUser> userManager, UserService userService)
        {
            _context = context;
            _userManager = userManager;
            _userService = userService;
        }

        public async Task<IActionResult> IndexAsync()
        {

            List<Tournament> tournaments = _context.Tournaments
        .OrderByDescending(x => x.StartDate)
        .Include(t => t.Users) // Eagerly load the Users collection
        .ToList();
            var nonAdminUsers = await _userService.GetNonAdminUsersAsync();
            ViewBag.userCount= nonAdminUsers.Count();
           ViewBag.tournamentCount= tournaments.Count();
           



            return View(tournaments);
        }
    }
}
