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

                        var user = await _userManager.FindByIdAsync(item.ToString());

                        model.Tournament.Users.Add(user);

                        // Create a TournamentUser entity and associate it with the tournament
                        var tournamentUser = new TournamentUser
                        {
                            Tournament = model.Tournament,
                            User = user,
                            // Set other properties like LoseCount, LifeCount, etc.
                        };

                        // Add the TournamentUser to the context
                        _context.TournamentUsers.Add(tournamentUser);
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
                List<AppUser> users = new List<AppUser>();

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
            }

            var nonAdminUsers = await _userService.GetNonAdminUsersAsync();
            TournamentCreateVM vm = new()
            {
                Users = nonAdminUsers,
                Tournament = model.Tournament
            };

            return View(vm);

        }


        public async Task<IActionResult> Tours(int tournamentId)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Users)
                .Include(t => t.Tours).ThenInclude(tour => tour.Users)
                .Include(t => t.Tours).ThenInclude(tour => tour.Pairs)
                .FirstOrDefaultAsync(x => x.Id == tournamentId);


            List<ScoreViewModel> scoresForTour = new List<ScoreViewModel>();
            var toursWithPairs = _context.Tours
            .Include(tour => tour.Pairs)
            .Where(t => t.TournamentId == tournamentId)
            .ToList();

            foreach (var tour in toursWithPairs)
            {
                var scoresForCurrentTour = InitializeScores(tour);
                scoresForTour.AddRange(scoresForCurrentTour);
              
            }

            List<PairVM> pairVms = new List<PairVM>();
            bool allPairsHaveZeroScore = false;


            if (toursWithPairs.Count() != 0)
            {
                var lastTour = toursWithPairs.Last();

                foreach (var pair in lastTour.Pairs)
                {
                    var scoresForCurrentPair = InitializeScores(lastTour);
                    scoresForTour.AddRange(scoresForCurrentPair);

                    // Calculate allPairsHaveZeroScore based on your logic
                    if(pair.User1Id!=0 && pair.User2Id != 0)
                    {
                        if (pair.User1Score == 0 && pair.User2Score == 0)
                        {
                            allPairsHaveZeroScore = true;
                            break; // No need to continue checking if any pair has a non-zero score
                        }
                    }
                 
                }


               pairVms = lastTour.Pairs
              .Select(pair => new PairVM
              {
                    // Map properties from Pair to PairVM
                  PairId = pair.Id,
                  User1 = _context.Users.FirstOrDefault(user => user.Id == pair.User1Id),
                  User2 = _context.Users.FirstOrDefault(user => user.Id == pair.User2Id),
                  Tour = pair.Tour

              })
              .ToList();

            }
           

            var vm = new ToursVM
            {
                Tournament = tournament,
                Scores = scoresForTour,
                PairVMs= pairVms,
                AllPairsHaveZeroScore = allPairsHaveZeroScore          
            };



            var champion = _context.TournamentChampions.Where(x => x.TournamentId == tournamentId).ToList();
            if (champion.Count() != 0)
            {
                var championUser = _context.Users
                   .FirstOrDefault(u => u.Id == champion.FirstOrDefault().ChampionId);
                TempData["ChampionName"] = $"{championUser.Name} {championUser.SurName}";
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> DrawLot(Tournament model, int id)
        {

            var tournament = await _context.Tournaments
                .Include(t => t.Users)
                .Include(t => t.Tours).ThenInclude(tour => tour.Users)
                .Include(t => t.Tours).ThenInclude(tour => tour.Pairs)
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            var pai = tournament.Tours.LastOrDefault().Pairs;
            foreach(var item in pai)
            {
                if(item.User1Id!=0 && item.User2Id != 0)
                {
                    if (item.User1Score == 0 && item.User2Score == 0) return RedirectToAction("Tours", new { tournamentId = model.Id });
                }
               
            }


            List<AppUser> eligibleUsers = await _userService.GetNonAdminUsersOfATournamentAsync(model.Id);
            if (eligibleUsers.Count < 2)
            {
                TempData["UserCount"] = "En az iki kişi olmalı";
                return RedirectToAction("Tours", new { tournamentId = model.Id });
            }

            List<AppUser> users = eligibleUsers
    .Where(user =>
    {
        var tournamentUser = _context.TournamentUsers
            .FirstOrDefault(tu => tu.UserId == user.Id && tu.TournamentId == model.Id);

        return tournamentUser == null || tournamentUser.LoseCount < model.PlayLife;
    })
    .ToList();

            if (users.Count == 1)
            {
                // Handle the case where there are only two users (e.g., display a message)
                TempData["ChampionName"] = $"{users.FirstOrDefault().Name} {users.FirstOrDefault().SurName}";


                var champion = new TournamentChampion
                {
                    TournamentId = model.Id,
                    ChampionId = users.FirstOrDefault().Id
                };

                _context.TournamentChampions.Add(champion);
                _context.SaveChanges();


                // Redirect to the Tours action or another appropriate action
                return RedirectToAction("Tours", new { tournamentId = model.Id });
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
            Random random = new Random();
            users = users.OrderBy(x => random.Next()).ToList();
            List<AppUser> shuffledUsers = users;

            int minByeCount = 0;
            AppUser lastone = new AppUser();
            if (users.Count() % 2 != 0)
            {
                shuffledUsers = users
                  .OrderByDescending(u => u.TournamentUsers.FirstOrDefault(tu => tu.TournamentId == model.Id)?.ByeCount ?? int.MaxValue)
                  .ToList();

                lastone = shuffledUsers.LastOrDefault();

                shuffledUsers = shuffledUsers.Except(new List<AppUser> { lastone }).ToList();
            }
            if (model.Type == "Kazananlar Eşleşir" || model.Type == "Kaybedenler Eşleşir" || model.Type == "Aynı Haklılar Eşleşir")
            {

                shuffledUsers = shuffledUsers
               .OrderBy(u => u.TournamentUsers.FirstOrDefault(tu => tu.TournamentId == model.Id)?.LoseCount ?? int.MaxValue)
               .ThenBy(u => random.Next()) // Use the same Random instance
               .ToList();


            }



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
                    shuffledUsers[i].Tours.Add(newTour);
                    shuffledUsers[i + 1].Tours.Add(newTour);
                    pair.User1Id = shuffledUsers[i].Id;
                    pair.User2Id = shuffledUsers[i + 1].Id;

                }
            }

            if (users.Count % 2 != 0)
            {


                // Create a pair with one user that has no rival
                Pair singleUserPair = new Pair
                {

                    User1Id = lastone.Id,
                    User2Id = 0
                };
                pairs.Add(singleUserPair);

                PairVM SingleUserPairvm = new()
                {
                    Tour = newTour,
                    User1 = lastone,
                    User2 = null,

                };
                pairVMs.Add(SingleUserPairvm);//
                lastone.Tours.Add(newTour);

                UpdateByeCount(lastone.Id, model.Id);
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

            List<ScoreViewModel> scoresForTour = new List<ScoreViewModel>();
            var toursWithPairs = _context.Tours
            .Include(tour => tour.Pairs)
            .Where(t => t.TournamentId == model.Id)
            .ToList();
            foreach (var tour in toursWithPairs)
            {
                var scoresForCurrentTour = InitializeScores(tour);
                scoresForTour.AddRange(scoresForCurrentTour);
            }

            ToursVM vm = new()
            {
                Tournament = await _context.Tournaments
                    .Include(t => t.Users) // Include the Users for the Tournament
                    .Include(t => t.Tours)
                        .ThenInclude(tour => tour.Users) // Include the Users for each Tour
                    .Include(t => t.Tours)
                        .ThenInclude(tour => tour.Pairs)
                    .FirstOrDefaultAsync(x => x.Id == model.Id),
                PairVMs = pairVMs,
                Scores = scoresForTour,
                SaveScore=true
            };

            return View("Tours", vm);
        }

        private string GetUserDisplayName(int userId)
        {
            var user = _context.Users.Find(userId);
            return user != null ? $"{user.Name} {user.SurName}" : "User not found";
        }

        private List<ScoreViewModel> InitializeScores(Tour tour)
        {
            return tour.Pairs.Select(pair => new ScoreViewModel
            {
                TournamentId = tour.TournamentId,
                PairId = pair.Id,
                User1Score = pair.User1Score,  // Fetch from the database
                User2Score = pair.User2Score,  // Fetch from the database
                User1Name = GetUserDisplayName(pair.User1Id),
                User2Name = GetUserDisplayName(pair.User2Id),
                TourId = tour.Id
            }).ToList();
        }


        [HttpPost]
        public IActionResult SaveScores(List<ScoreViewModel> scores)
        {
            if (ModelState.IsValid)
            {

                foreach (var score in scores)
                {
                    // Retrieve the pair from the database

                    var pairOfScore = _context.Pairs.Where(x => x.Id == score.PairId).FirstOrDefault();


                    if (pairOfScore.User1Id != 0 && pairOfScore.User2Id != 0)
                    {
                        if (score.User1Score == 0 && score.User2Score == 0)
                        {
                            return RedirectToAction("Tours", new { tournamentId = score.TournamentId }); // Replace with your actual view name and view model
                        }

                    }
                }

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


                                //Determine the loser and update the loseCount for the current tournament
                                if (pair.User1Score < pair.User2Score)
                                {
                                    UpdateLoseCount(pair.User1Id, score.TournamentId);
                                    UpdateWinCount(pair.User2Id, score.TournamentId);
                                }
                                else if (pair.User1Score > pair.User2Score)
                                {
                                    UpdateLoseCount(pair.User2Id, score.TournamentId);
                                    UpdateWinCount(pair.User1Id, score.TournamentId);
                                }

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
            return View("Tours", new ToursVM{
                DrawLot = true
            }); // Replace with your actual view name and view model
        }

        private void UpdateLoseCount(int userId, int tournamentId)
        {
            var tournamentUser = _context.TournamentUsers.FirstOrDefault(tu => tu.UserId == userId && tu.TournamentId == tournamentId);
            if (tournamentUser != null)
            {
                tournamentUser.LoseCount++;
                _context.TournamentUsers.Update(tournamentUser);
            }
        }

        private void UpdateWinCount(int userId, int tournamentId)
        {
            var tournamentUser = _context.TournamentUsers.FirstOrDefault(tu => tu.UserId == userId && tu.TournamentId == tournamentId);
            if (tournamentUser != null)
            {
                tournamentUser.WinCount++;
                _context.TournamentUsers.Update(tournamentUser);
            }
        }
        private void UpdateByeCount(int userId, int tournamentId)
        {
            var tournamentUser = _context.TournamentUsers.FirstOrDefault(tu => tu.UserId == userId && tu.TournamentId == tournamentId);
            if (tournamentUser != null)
            {
                tournamentUser.ByeCount++;
                _context.TournamentUsers.Update(tournamentUser);
            }
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
            var tournament = await _context.Tournaments
        .Include(t => t.Users) // Include users associated with the tournament
        .FirstOrDefaultAsync(x => x.Id == tournamentId);

            var allUsers = await _userService.GetNonAdminUsersAsync();

            // Filter users who are already associated with the tournament
            var usersInTournament = allUsers.Where(user => tournament.Users.Any(tUser => tUser.Id == user.Id)).ToList();

            ViewBag.AllUsers = allUsers;
            ViewBag.UsersInTournament = usersInTournament;

            return View(tournament);

        }
        [HttpPost]
        public async Task<IActionResult> UpdateT(Tournament model, int[] selectedUsers)
        {
            var tournament = await _context.Tournaments
                            .Include(t => t.TournamentUsers)
                            .Include(t => t.Users)  // Explicitly include the Users collection
                            .FirstOrDefaultAsync(x => x.Id == model.Id);
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


            // Add new users to both AppUserTournament and TournamentUsers
            foreach (var userId in selectedUsers)
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                // Add user to AppUserTournament if not already present
                if (!tournament.Users.Any(u => u.Id == user.Id))
                {
                    tournament.Users.Add(user);
                }

                // Add user to TournamentUsers if not already present
                if (!tournament.TournamentUsers.Any(tu => tu.UserId == user.Id))
                {
                    var tournamentUser = new TournamentUser
                    {
                        TournamentId = tournament.Id,
                        UserId = user.Id,
                        // Set other properties like LoseCount, LifeCount, etc.
                    };

                    _context.TournamentUsers.Add(tournamentUser);
                }
            }


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
            return RedirectToAction("UpdateT", new { tournamentId = tournament.Id });
        }
        public async Task<IActionResult> ExcludeFromTournament(int userId, int tournamentId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var tournament = await _context.Tournaments
                .Include(t => t.Users)
                .Include(t => t.TournamentUsers)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (user != null && tournament != null)
            {
                // Remove the user from the Users collection of the tournament
                tournament.Users.Remove(user);

                // Remove the user from the TournamentUsers table
                var tournamentUser = tournament.TournamentUsers.FirstOrDefault(tu => tu.UserId == userId);
                if (tournamentUser != null)
                {
                    tournament.TournamentUsers.Remove(tournamentUser);
                }

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "User excluded from the tournament successfully!";
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.Message);
                }
            }
            else
            {
                TempData["ErrorMessage"] = "User or tournament not found.";
            }

            return RedirectToAction("UpdateT", new { tournamentId = tournamentId });
        }


        public async Task<IActionResult> TUserList(int tournamentId)
        {
            var tournamentUsers = await _context.TournamentUsers
                .Where(tu => tu.TournamentId == tournamentId)
                .OrderByDescending(tu => tu.WinCount)
                .Include(tu => tu.User) // Eagerly load the User entity
                .ToListAsync();
            return View(tournamentUsers);
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

