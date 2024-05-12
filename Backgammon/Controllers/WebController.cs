using Backgammon.Business;
using Backgammon.Context;
using Backgammon.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backgammon.Controllers
{
    public class WebController : Controller
    {
        private readonly BackgammonContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly UserService _userService;

        public WebController(BackgammonContext context, UserManager<AppUser> userManager, UserService userService)
        {
            _context = context;
            _userManager = userManager;
            _userService = userService;
        }

        public IActionResult Index()
        {
            List<Tournament> tournaments = _context.Tournaments
       .OrderByDescending(x => x.StartDate)
       .Include(t => t.Users) // Eagerly load the Users collection
       .ToList();
            return View(tournaments);
        }
        public async Task<IActionResult> TUserList(int tournamentId)
        {
            var tournamentUsers = await _context.TournamentUsers
                .Where(tu => tu.TournamentId == tournamentId)
                .OrderByDescending(tu => tu.WinCount)
                .ThenBy(tu => tu.LoseCount)
                .Include(tu => tu.User)
                .Include(tu => tu.Tournament)// Eagerly load the User entity
                .ToListAsync();

            ViewBag.TournamentId = tournamentId;
            return View(tournamentUsers);
        }

    }
}
