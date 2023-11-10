using Backgammon.Business;
using Backgammon.Context;
using Backgammon.Entities;
using Backgammon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backgammon.Controllers
{

    public class TournamentController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly UserService _userService;
        private readonly BackgammonContext _context;


        public TournamentController(UserManager<AppUser> userManager, UserService userService, BackgammonContext context)
        {
            _userManager = userManager;
            _userService = userService;
            _context = context;
        }

        public async Task<IActionResult> CreateTAsync()
        {
            var nonAdminUsers = await _userService.GetNonAdminUsersAsync();
            TournamentCreateVM vm = new()
            {
                Users = nonAdminUsers
            };
            return View(vm);

        }

        [HttpPost]
        public async Task<IActionResult> CreateT(TournamentCreateVM model, int[] Id)
        {
            if (ModelState.IsValid)
            {
                // Save or update Season
                if (model.Tournament.Id == 0)
                {
                    model.Tournament.CreateDate = DateTime.Now;

                    foreach (int item in Id)
                    {

                        model.Tournament.Users.Add(await _userManager.FindByIdAsync(item.ToString()));
                    }



                    _context.Tournaments.Add(model.Tournament);
                }
                else
                {
                    _context.Tournaments.Update(model.Tournament);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("GetListT"); // Redirect to index or another appropriate action
            }
            else
            {

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
            }

            List<AppUser> users = _context.Users.ToList();

            model.Tournament.Users = users;

            return View(model);
            ;
        }


        public async Task<IActionResult> Tours(int tournamentId)
        {
            var tournament = await _context.Tournaments
       .Include(t => t.Tours)
           .ThenInclude(t => t.Pairs)
       .FirstOrDefaultAsync(x => x.Id == tournamentId);

            var vm = new ToursVM
            {
                Tournament = tournament,
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> DrawLot(Tournament model, int id)
        {
            List<AppUser> users = await _userService.GetNonAdminUsersOfATournamentAsync(model.Id);
            if (users.Count < 2)
            {
                // Handle the case where there are not enough users to create pairs.
                // You may want to return a view with an error message in this case.
                return View();
            }

            int existingTourCount = await _context.Tours
      .Where(t => t.TournamentId == model.Id)
      .CountAsync();

            // Generate the name for the new Tour
            string newTourName = $"Tur{existingTourCount + 1}";

            // Create a new Tour with the generated name
            var newTour = new Tour
            {
                Name = newTourName,
                TournamentId = model.Id
            };
            _context.Tours.Add(newTour);
            await _context.SaveChangesAsync();

            // Shuffle the list of users to create random pairs.
            Random random = new Random();
            List<AppUser> shuffledUsers = users.OrderBy(x => random.Next()).ToList();

            List<Pair> pairs = new List<Pair>();
            List<PairVM> pairVMs = new List<PairVM>();

            for (int i = 0; i < shuffledUsers.Count; i += 2)
            {
                if (i + 1 < shuffledUsers.Count)
                {
                    Pair pair = new Pair
                    {

                        User1Id = shuffledUsers[i].Id,
                        User2Id = shuffledUsers[i + 1].Id
                    };
                    pairs.Add(pair);

                    PairVM pairvm = new()
                    {
                        Tour = newTour,
                        User1 = shuffledUsers[i],
                        User2 = shuffledUsers[i + 1],

                    };
                    pairVMs.Add(pairvm);
                }
            }

            if (shuffledUsers.Count % 2 != 0)
            {
                // Create a pair with one user that has no rival
                Pair singleUserPair = new Pair
                {

                    User1Id = shuffledUsers.Last().Id,
                    User2Id = 0
                };
                pairs.Add(singleUserPair);

                PairVM SingleUserPairvm = new()
                {
                    Tour = newTour,
                    User1 = shuffledUsers.Last(),
                    User2 = null,

                };
                pairVMs.Add(SingleUserPairvm);
            }


            // You can do something with the pairs, like saving them to a database or displaying them in a view.
            // For now, let's assume you want to pass the pairs to a view.
            foreach (var pair in pairs)
            {
                pair.TourId = newTour.Id; // Set the TourId to the newly generated Tour's Id             
                _context.Pairs.Add(pair);
            }
            await _context.SaveChangesAsync();

            var savedPairs = _context.Pairs.Where(p => p.TourId == newTour.Id).ToList();

            List<ScoreViewModel> scores = savedPairs.Select(pair => new ScoreViewModel
            {
                TournamentId = model.Id,
                PairId = pair.Id, // Set the PairId using the Id of the saved Pair entity
                User1Score = 0,   // Initialize with default values
                User2Score = 0    // Initialize with default values
            }).ToList();

            ToursVM vm = new()
            {
                Tournament = await _context.Tournaments
       .Include(t => t.Tours)
           .ThenInclude(t => t.Pairs)
       .FirstOrDefaultAsync(x => x.Id == model.Id),
                PairVMs = pairVMs,
                Scores = scores
            };

            return View("Tours", vm);
        }


        [HttpPost]
        public IActionResult SaveScores(List<ScoreViewModel> scores)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var score in scores)
                        {
                            // Retrieve the pair from the database
                            var pair = _context.Pairs.FirstOrDefault(p => p.Id == score.PairId);

                            if (pair != null)
                            {
                                // Update the scores
                                pair.User1Score = score.User1Score;
                                pair.User2Score = score.User2Score;

                                // Update the pair in the database
                                _context.Pairs.Update(pair);
                            }
                        }

                        // Save changes to the database
                        _context.SaveChanges();

                        // Commit the transaction
                        transaction.Commit();
                        int id = scores.FirstOrDefault()?.TournamentId ?? 0;
                        return RedirectToAction("Tours", new { tournamentId = id }); // Redirect to a success page or another action
                    }
                    catch (Exception ex)
                    {
                        // Handle any errors that may occur during the transaction
                        transaction.Rollback();

                        // Log or handle the exception as needed
                        ModelState.AddModelError(string.Empty, "An error occurred while saving scores.");
                    }
                }
            }
            else
            {

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
            }

            // If ModelState is not valid, return to the form with validation errors
            return View("Tours", new ToursVM()); // Replace with your actual view name and view model
        }


        public IActionResult GetListT()
        {
            List<Tournament> tournaments = _context.Tournaments
        .OrderByDescending(x => x.StartDate)
        .Include(t => t.Users) // Eagerly load the Users collection
        .ToList();


            return View(tournaments);
        }



        public async Task<IActionResult> UpdateT(int tournamentId)
        {
            var t = _context.Tournaments.FirstOrDefault(x => x.Id == tournamentId);

            return View(t);

        }
        [HttpPost]
        public async Task<IActionResult> UpdateT(Tournament model)
        {
            var tournament = await _context.Tournaments.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (tournament == null)
            {
                return NotFound();
            }
            tournament.Name = model.Name ?? "";
            tournament.StartDate = model.StartDate;
            tournament.Place = model.Place;
            tournament.System = model.System;
            tournament.Type = model.Type;
            tournament.PlayLife = model.PlayLife;
            tournament.ByeType = model.ByeType;
            tournament.TableStart = model.TableStart;
            try
            {
                var result = _context.Tournaments.Update(tournament);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Turnuva başarıyla güncellendi!";
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return View();
            }
            return View(tournament);
        }

        public async Task<IActionResult> TUserList(int tournamentId)
        {
            List<AppUser> users = await _userService.GetNonAdminUsersOfATournamentAsync(tournamentId);
            return View(users);
        }

        public async Task<IActionResult> DeleteT(int tournamentId)
        {
            Tournament t = await _context.Tournaments.FirstOrDefaultAsync(x => x.Id == tournamentId);
            var result = _context.Tournaments.Remove(t);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Turnuva başarıyla silindi!";
            return RedirectToAction("GetListT");

        }
    }
}

