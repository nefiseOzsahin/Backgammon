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
                .ThenBy(tu => tu.User.Name)
                .ThenBy(tu => tu.User.SurName)
                .Include(tu => tu.User)
                .Include(tu => tu.Tournament)// Eagerly load the User entity
                .ToListAsync();

            ViewBag.TournamentId = tournamentId;

            var tours =  _context.Tours.Where(x => x.TournamentId == tournamentId);
            ViewBag.tours = tours;

            return View(tournamentUsers);
        }

        [HttpPost]
        public ActionResult GetPairs(string selectedValue)
        {
            try
            {
                if (!int.TryParse(selectedValue, out int tourId))
                {
                    return Json(new { success = false, message = "Invalid tour ID" });
                }

                var pairs = from pair in _context.Pairs
                            join user1 in _context.Users on pair.User1Id equals user1.Id
                            join user2 in _context.Users on pair.User2Id equals user2.Id
                            where pair.TourId == tourId
                            select new
                            {
                                User1Name = user1.Name+" "+user1.SurName,
                                User1Score = pair.User1Score,
                                User2Name = user2.Name + " " + user2.SurName,
                                User2Score = pair.User2Score
                            };

                var result = pairs.ToList().Select(p => new
                {
                    p.User1Name,
                    p.User1Score,
                    p.User2Name,
                    p.User2Score
                });

                return Json(new { success = true, pairs = result });
            }
            catch (Exception ex)
            {
                // Log the exception (ex)
                return Json(new { success = false, message = "An error occurred while processing your request." });
            }
        }




    }
}
