using Backgammon.Business;
using Backgammon.Context;
using Backgammon.Entities;
using Backgammon.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Backgammon.Controllers
{

    [Authorize(Roles = "Admin")]
    public class TournamentController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly UserService _userService;
        private readonly BackgammonPairing _pairing;
        private readonly BackgammonContext _context;
      



        public TournamentController(UserManager<AppUser> userManager, UserService userService, BackgammonPairing pairing,BackgammonContext context)
        {
            _userManager = userManager;
            _userService = userService;
            _pairing = pairing;
            _context = context;
          
        }
       
        public async Task<IActionResult> CreateT()
        {
            var nonAdminUsers = await _userService.GetNonAdminUsersAsync();
            TournamentCreateVM vm = new()
            {
                Users = nonAdminUsers
            };
            return View(vm);

        }


        public async Task<IActionResult> Duplication(int tournamentId,int lastUserId)
        {

            ToursVM vm = new()
            {
                Tournament = await _context.Tournaments
                  .Include(t => t.Users) // Include the Users for the Tournament
                  .Include(t => t.Tours)
                      .ThenInclude(tour => tour.Users) // Include the Users for each Tour
                  .Include(t => t.Tours)
                      .ThenInclude(tour => tour.Pairs)
                  .FirstOrDefaultAsync(x => x.Id == tournamentId)//,
                //PairVMs = pairVMs,
                //Scores = scoresForTour,
                //SaveScore = true
            };

            ViewBag.lastUserId = lastUserId;

            return View("Duplication", vm);

        }

        [HttpPost]
        public async Task<IActionResult> CreateT(TournamentCreateVM model, int[] Id)
        {
            if (ModelState.IsValid)
            {
                // Capitalize the first letter of each word in the Tournament name
                model.Tournament.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.Tournament.Name.ToLower());
                // Capitalize the first letter of each word in the Tournament name
                model.Tournament.Place = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.Tournament.Place.ToLower());

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
                        SMSSendTournamentRegister(user, model.Tournament.Name);
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
            bool allNonZeroScores = false;

            var lastTour = new Tour();

            if (toursWithPairs.Count() != 0)
            {
                lastTour = toursWithPairs.Last();

                foreach (var pair in lastTour.Pairs)
                {
                    var scoresForCurrentPair = InitializeScores(lastTour);
                    scoresForTour.AddRange(scoresForCurrentPair);

                    // Calculate allPairsHaveZeroScore based on your logic
                    if (pair.User1Id != 0 && pair.User2Id != 0)
                    {
                        if (pair.User1Score == 0 && pair.User2Score == 0)
                        {
                            allPairsHaveZeroScore = true;
                            allNonZeroScores = true;
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
                PairVMs = pairVms,
                AllPairsHaveZeroScore = allPairsHaveZeroScore
            };




            //if (!allNonZeroScores)
            //{
            //    foreach (var pair in lastTour.Pairs)
            //    {


            //        //Determine the loser and update the loseCount for the current tournament
            //        if (pair.User1Score < pair.User2Score)
            //        {
            //            UpdateLoseCount(pair.User1Id, tournamentId, lastTour.Id);
            //            UpdateWinCount(pair.User2Id, tournamentId, lastTour.Id);
            //        }
            //        else if (pair.User1Score > pair.User2Score)
            //        {
            //            UpdateLoseCount(pair.User2Id, tournamentId, lastTour.Id);
            //            UpdateWinCount(pair.User1Id, tournamentId, lastTour.Id);
            //        }


            //    }
            //}
            _context.SaveChanges();

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
        public async Task<IActionResult> DrawLot(Tournament model, string source,int lastUserId)
        {
         
         

            var tournament = await _context.Tournaments
                .Include(t => t.Users)
                .Include(t => t.Tours).ThenInclude(tour => tour.Users)
                .Include(t => t.Tours).ThenInclude(tour => tour.Pairs)
                .FirstOrDefaultAsync(x => x.Id == model.Id);
            UpdateIsSMSSendFalse(tournament.Id);
            //Son turda duplication olduğu tespit edilmiş, kullanıcıya bu turu tekrar çekmek istermisiniz
            //diye sorulmuş, kullanıcıda Evet butonuna basarak son turu silip yeniden tur oluşturmak istemiştir.
            if (source == "EvetButton")
            {
                var lastTour = tournament.Tours.LastOrDefault();
                if (lastTour != null)
                {
                    // Son tur varsa, bu turu kaldırabilirsiniz.
                    _context.Tours.Remove(lastTour);
                    await _context.SaveChangesAsync();
                    if (lastUserId != 0)
                    {
                        DecreaseByeCount(model.Id,lastUserId, lastTour.Id);
                    }
                }
            }
            //-------------------------------------------------------------
            if (tournament.Tours.Count() != 0)
            {
                var pai = tournament.Tours.LastOrDefault().Pairs;
                foreach (var item in pai)
                {
                    if (item.User1Id != 0 && item.User2Id != 0)
                    {
                        if (item.User1Score == 0 && item.User2Score == 0) return RedirectToAction("Tours", new { tournamentId = model.Id });
                    }

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
            users = users.OrderBy(x => Guid.NewGuid()).ToList();



            List<AppUser> shuffledUsers = users;

            int minByeCount = 0;
            AppUser lastone = new AppUser();
            if (users.Count() % 2 != 0)
            {
                shuffledUsers = users
                  .OrderByDescending(u => u.TournamentUsers.FirstOrDefault(tu => tu.TournamentId == model.Id)?.ByeCount ?? int.MaxValue)
                  .ToList();

                //lastone = shuffledUsers.LastOrDefault();
                lastone = shuffledUsers
    .OrderBy(u => u.TournamentUsers.FirstOrDefault(tu => tu.TournamentId == model.Id)?.ByeCount ?? int.MaxValue)
    .ThenBy(u => u.TournamentUsers.FirstOrDefault(tu => tu.TournamentId == model.Id)?.WinCount ?? int.MaxValue)
    .FirstOrDefault();

                shuffledUsers = shuffledUsers.Except(new List<AppUser> { lastone }).ToList();
            }
            if (model.Type == "Kazananlar Eşleşir" || model.Type == "Kaybedenler Eşleşir" || model.Type == "Aynı Haklılar Eşleşir")
            {

           

                // Shuffling within groups of users with the same LoseCount
                var rnd = new Random();
                //shuffledUsers = shuffledUsers
                //.GroupBy(u => (
                //    loseCount: u.TournamentUsers.FirstOrDefault(tu => tu.TournamentId == model.Id)?.LoseCount ?? int.MinValue,
                //    winCount: u.TournamentUsers.FirstOrDefault(tu => tu.TournamentId == model.Id)?.WinCount ?? int.MaxValue))
                //.SelectMany(grp => grp.OrderBy(x => rnd.Next()))
                //.ToList();
                shuffledUsers = shuffledUsers
                .OrderByDescending(u => u.TournamentUsers.FirstOrDefault(tu => tu.TournamentId == model.Id)?.WinCount)
                .ThenBy(u => u.TournamentUsers.FirstOrDefault(tu => tu.TournamentId == model.Id)?.LoseCount)
                .ToList();
            }



            List<Pair> pairs = new List<Pair>();
            List<PairVM> pairVMs = new List<PairVM>();
        
         

            var pairedUsers = new HashSet<string>();
            //var totalUsers = shuffledUsers.Count;
            //var totalPossiblePairings = totalUsers * (totalUsers - 1) / 2;
            //var maxIterations = Math.Min(totalUsers / 2, totalPossiblePairings);
            var triedUsers = new List<AppUser>();

            do
            {
                var allPairs = _context.Pairs.Where(p => p.Tour.TournamentId == model.Id).ToList();
            
                // Check if there are at least two users left to pair
                if (shuffledUsers.Count >= 2)
                {
                    var user1 = shuffledUsers.First();
                    var user2 = shuffledUsers.Skip(1).First();


                   
                        // Check if the pair already exists in previous tours
                        var existingPair = allPairs.FirstOrDefault(p =>
                            (p.User1Id == user1.Id && p.User2Id == user2.Id) ||
                            (p.User1Id == user2.Id && p.User2Id == user1.Id));
                        if (existingPair == null)
                        {

                            var pair = new Pair { User1Id = user1.Id, User2Id = user2.Id };

                            var pairVM = new PairVM { Tour = newTour, User1 = user1, User2 = user2 };

                            // Add the pair to the database
                            pair.TourId = newTour.Id;
                            _context.Pairs.Add(pair);

                            // Add the pair view model to the list
                            pairVMs.Add(pairVM);

                            // Add the tour to the users
                            user1.Tours.Add(newTour);
                            user2.Tours.Add(newTour);

                            // Mark users as paired to avoid duplicate pairings
                            pairedUsers.Add(user1.Id.ToString());
                            pairedUsers.Add(user2.Id.ToString());
                            shuffledUsers = shuffledUsers.Skip(2).ToList();                       
                          
                        }
                        else
                        {
                        //shuffledUsers = shuffledUsers.OrderBy(x => Guid.NewGuid()).ToList();
                        shuffledUsers = shuffledUsers.Where(u => u != user2).ToList();
                        triedUsers.Add(user2);
                        continue;
                        }              
                  

                  

                }else if (shuffledUsers.Count() == 1)
                {
                    if (triedUsers != null)
                    {
                        triedUsers = triedUsers.OrderBy(x => Guid.NewGuid()).ToList();
                        var user1 = shuffledUsers.First();
                        var user2 = triedUsers.First();
                        var pair = new Pair { User1Id = user1.Id, User2Id = user2.Id };
                        var pairVM = new PairVM { Tour = newTour, User1 = user1, User2 = user2 };
                        pair.TourId = newTour.Id;
                        _context.Pairs.Add(pair);

                        // Add the pair view model to the list
                        pairVMs.Add(pairVM);

                        // Add the tour to the users
                        user1.Tours.Add(newTour);
                        user2.Tours.Add(newTour);

                        // Mark users as paired to avoid duplicate pairings
                        pairedUsers.Add(user1.Id.ToString());
                        pairedUsers.Add(user2.Id.ToString());
                        shuffledUsers = shuffledUsers.Skip(1).ToList();
                        triedUsers = triedUsers.Skip(1).ToList();
                    }
                   
                }else if(shuffledUsers.Count() == 0 && triedUsers.Count() >= 2)
                {
                    shuffledUsers = triedUsers;
                    triedUsers = new List<AppUser>();
                    continue;

                }
                await _context.SaveChangesAsync();
                // Retrieve the saved pairs for the new tour
                var savedPairs = _context.Pairs.Where(p => p.TourId == newTour.Id).ToList();              

               
            } while (shuffledUsers.Any() || triedUsers.Any()/*&& pairedUsers.Count < maxIterations*/);
           
            if (users.Count() % 2 != 0)
            {
                var pair = new Pair { User1Id = lastone.Id, User2Id = 0 };
                var pairVM = new PairVM { Tour = newTour, User1 = lastone, User2 = null };

                // Add the pair to the database
                pair.TourId = newTour.Id;
                _context.Pairs.Add(pair);

                // Add the pair view model to the list
                pairVMs.Add(pairVM);

                // Add the tour to the user
                lastone.Tours.Add(newTour);

                // Update bye count
                UpdateByeCountAsync(lastone.Id, model.Id,tournament);

                // Mark the bye user as paired
                pairedUsers.Add(lastone.Id.ToString());
                await _context.SaveChangesAsync();

            }
            // Handle case for odd number of players
            //if (shuffledUsers.Count % 2 != 0)
            //{
            //    var byeUser = shuffledUsers.LastOrDefault();
            //    if (byeUser != null)
            //    {
            //        // Ensure the bye user is not already paired
            //        if (!pairedUsers.Contains(byeUser.Id.ToString()))
            //        {
            //            var pair = new Pair { User1Id = byeUser.Id, User2Id = 0 };
            //            var pairVM = new PairVM { Tour = newTour, User1 = byeUser, User2 = null };

            //            // Add the pair to the database
            //            pair.TourId = newTour.Id;
            //            _context.Pairs.Add(pair);

            //            // Add the pair view model to the list
            //            pairVMs.Add(pairVM);

            //            // Add the tour to the user
            //            byeUser.Tours.Add(newTour);

            //            // Update bye count
            //            UpdateByeCount(byeUser.Id, model.Id);

            //            // Mark the bye user as paired
            //            pairedUsers.Add(byeUser.Id.ToString());
            //        }
            //    }
            //}

            // Save changes to the database
            //await _context.SaveChangesAsync();

            //// Retrieve the saved pairs for the new tour
            //var savedPairs = _context.Pairs.Where(p => p.TourId == newTour.Id).ToList();

            /////////////////////////////////////////*******************************

            //for (int i = 0; i < shuffledUsers.Count; i += 2)
            //{
            //    if (i + 1 < shuffledUsers.Count)
            //    {
            //        Pair pair = new Pair
            //        {

            //            User1Id = shuffledUsers[i].Id,
            //            User2Id = shuffledUsers[i + 1].Id
            //        };
            //        pairs.Add(pair);

            //        PairVM pairvm = new()
            //        {
            //            Tour = newTour,
            //            User1 = shuffledUsers[i],
            //            User2 = shuffledUsers[i + 1],

            //        };
            //        pairVMs.Add(pairvm);
            //        shuffledUsers[i].Tours.Add(newTour);
            //        shuffledUsers[i + 1].Tours.Add(newTour);
            //        pair.User1Id = shuffledUsers[i].Id;
            //        pair.User2Id = shuffledUsers[i + 1].Id;

            //    }
            //}

            //if (users.Count % 2 != 0)
            //{


            //    // Create a pair with one user that has no rival
            //    Pair singleUserPair = new Pair
            //    {

            //        User1Id = lastone.Id,
            //        User2Id = 0
            //    };
            //    pairs.Add(singleUserPair);

            //    PairVM SingleUserPairvm = new()
            //    {
            //        Tour = newTour,
            //        User1 = lastone,
            //        User2 = null,

            //    };
            //    pairVMs.Add(SingleUserPairvm);//
            //    lastone.Tours.Add(newTour);

            //    UpdateByeCount(lastone.Id, model.Id);
            //}


            //// You can do something with the pairs, like saving them to a database or displaying them in a view.
            //// For now, let's assume you want to pass the pairs to a view.
            //foreach (var pair in pairs)
            //{
            //    pair.TourId = newTour.Id; // Set the TourId to the newly generated Tour's Id             
            //    _context.Pairs.Add(pair);
            //}
            //await _context.SaveChangesAsync();

            //var savedPairs = _context.Pairs.Where(p => p.TourId == newTour.Id).ToList();
            var isDuplicate = checkForDublication(tournament);
            if (isDuplicate)
            {

                return RedirectToAction("Duplication", new { tournamentId = model.Id, lastUserId=lastone.Id });

            }
           
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
                SaveScore = true
            };

            return View("Tours", vm);
        }

        private bool checkForDublication(Tournament tournament)
        {
            // Turnuvanın tüm turlarının ID'lerini toplayacak bir liste oluşturun
            var tourIds = tournament.Tours.Select(t => t.Id).ToList();

            // Son turun çiftlerini alacak bir liste oluşturun
            var lastTourId = tourIds.Last();
            var lastTourPairs = tournament.Tours.Single(t => t.Id == lastTourId).Pairs.ToList();

            // Tüm turlardaki çiftleri toplamak için bir liste oluşturun
            var allPairs = new List<Pair>();

            foreach (var tour in tournament.Tours)
            {
                // Her turdaki çiftleri allPairs listesine ekleyin
                allPairs.AddRange(tour.Pairs);
            }

            // Karşılaştırma yapmak için tüm çiftleri dolaşın
            foreach (var pair in allPairs)
            {
                // Son turdaki çiftleri karşılaştırmamaya dikkat edin
                if (pair.TourId != lastTourId)
                {
                    // Gerekli karşılaştırma yapılır, eğer eşleşme bulunursa true döndürülür
                    if ((lastTourPairs.Any(p => p.User1Id == pair.User1Id && p.User2Id == pair.User2Id) ||
                         lastTourPairs.Any(p => p.User1Id == pair.User2Id && p.User2Id == pair.User1Id)))
                    {
                        return true;
                    }
                }
            }

            // Hiçbir tekrarlanma bulunmadıysa false döndürün
            return false;
        }







        private string GetUserDisplayName(int userId)
        {
            var user = _context.Users.Find(userId);
            return user != null ? $"{user.Name} {user.SurName}" : "Bye";
        }

        private string GetUserPhoneNumber(int userId)
        {
            var user = _context.Users.Find(userId);
            return user != null ? $"{user.PhoneNumber}" : "Bye";
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
                User1PhoneNumber = GetUserPhoneNumber(pair.User1Id),
                User2PhoneNumber = GetUserPhoneNumber(pair.User2Id),
                TourId = tour.Id
            }).ToList();
        }


        [HttpPost]
        public IActionResult SaveScores(List<ScoreViewModel> scores, string actionSource)
        {

            var toursWithPairs = _context.Tours         
          .Where(t => t.TournamentId == scores.FirstOrDefault().TournamentId)
          .ToList();

            var lastTour = toursWithPairs.Last().Id;


            if (actionSource == null)
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

                        


                                //Determine the loser and update the loseCount for the current tournament
                                if (pair.User1Score < pair.User2Score)
                                {
                                    UpdateLoseCount(pair.User1Id, score.TournamentId,lastTour);
                                    UpdateWinCount(pair.User2Id, score.TournamentId,lastTour);
                                }
                                else if (pair.User1Score > pair.User2Score)
                                {
                                    UpdateLoseCount(pair.User2Id, score.TournamentId, lastTour);
                                    UpdateWinCount(pair.User1Id, score.TournamentId, lastTour);
                                }

                                if(!(pair.User1Score==0 && pair.User2Score == 0))
                                {
                                    UpdateIsSMSSend(score.TournamentId, pair, lastTour - 1);
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
                // Serialize scores to JSON string
                string scoresJson = JsonConvert.SerializeObject(scores);

                // Store scores JSON string in session
                HttpContext.Session.SetString("ScoresData", scoresJson);
               
                return RedirectToAction("SMSSend", new { tourCount = toursWithPairs.Count() });
                }            

            // If ModelState is not valid, return to the form with validation errors
            return View("Tours", new ToursVM
            {
                DrawLot = true
            }); // Replace with your actual view name and view model
        }

        private void UpdateLoseCount(int userId, int tournamentId, int lastTourId)
        {
            var tournamentUser = _context.TournamentUsers.FirstOrDefault(tu => tu.UserId == userId && tu.TournamentId == tournamentId);
            if (tournamentUser != null)
            {
                if (tournamentUser.TourId != lastTourId)
                {
                    tournamentUser.LoseCount++;
                    tournamentUser.TourId = lastTourId;
                    _context.TournamentUsers.Update(tournamentUser);
                }
            }
        }

        private void UpdateWinCount(int userId, int tournamentId, int lastTourId)
        {
            var tournamentUser = _context.TournamentUsers.FirstOrDefault(tu => tu.UserId == userId && tu.TournamentId == tournamentId);
            if (tournamentUser != null)
            {
                if (tournamentUser.TourId != lastTourId)
                {
                    tournamentUser.WinCount++;
                    tournamentUser.TourId = lastTourId;
                    _context.TournamentUsers.Update(tournamentUser);
                }
            }
        }
        private void UpdateByeCountAsync(int userId, int tournamentId,Tournament tournament)
        {
            var tournamentUser = _context.TournamentUsers.FirstOrDefault(tu => tu.UserId == userId && tu.TournamentId == tournamentId);
            if (tournamentUser != null)
            {
               
                var lastTour = tournament.Tours.LastOrDefault();
                if (tournamentUser.TourId != lastTour.Id)
                {
                    tournamentUser.ByeCount++;
                    tournamentUser.TourId = lastTour.Id;
                    _context.TournamentUsers.Update(tournamentUser);
                }
            }
        }


       

        private void DecreaseByeCount(int tournamentId,int userId,int lastTourId)
        {
            var tournamentUser = _context.TournamentUsers.FirstOrDefault(tu => tu.UserId == userId && tu.TournamentId == tournamentId);
            if (tournamentUser != null)
            {
                if (tournamentUser.TourId == lastTourId)
                {
                    tournamentUser.ByeCount--;
                    tournamentUser.TourId = lastTourId;
                    _context.TournamentUsers.Update(tournamentUser);
                }
            }

        }

        private void UpdateIsSMSSend(int tournamentId, Pair pair, int lastTourId)
        {
            var tournamentUser = _context.TournamentUsers.FirstOrDefault(tu => tu.UserId == pair.User1Id && tu.TournamentId == tournamentId);
            var tournamentUser2 = _context.TournamentUsers.FirstOrDefault(tu => tu.UserId == pair.User2Id && tu.TournamentId == tournamentId);
          
                if (tournamentUser != null)
                {
                    if (!tournamentUser.IsSMSSend)
                    {
                        SMSSendSaveScore(pair,tournamentId);
                            if (tournamentUser.TourId != lastTourId)
                            {
                                tournamentUser.IsSMSSend = true;
                             
                                if(tournamentUser2 != null)
                                {
                                    tournamentUser2.IsSMSSend = true;
                            
                                }
                                
                        _context.TournamentUsers.Update(tournamentUser);
                            }
                    }
                }
        }

        

        private void UpdateIsSMSSendFalse(int tournamentId)
        {

            var tournamentUsers = _context.TournamentUsers
                                   .Where(tu => tu.TournamentId == tournamentId);

            foreach (var user in tournamentUsers)
            {
                user.IsSMSSend = false;
            }

            _context.SaveChanges();

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
        .Include(t => t.Tours)
        .FirstOrDefaultAsync(x => x.Id == tournamentId);

            var allUsers = await _userService.GetNonAdminUsersAsync();

            // Filter users who are already associated with the tournament
            var usersInTournament = allUsers.Where(user => tournament.Users.Any(tUser => tUser.Id == user.Id)).ToList();

            ViewBag.AllUsers = allUsers;
            ViewBag.UsersInTournament = usersInTournament;

            return View(tournament);

        }
        [HttpPost]
        public async Task<IActionResult> UpdateT(Tournament model, int[] selectedUsers, int pageNum)
        {
            var tournament = await _context.Tournaments
                            .Include(t => t.TournamentUsers)
                            .Include(t => t.Users)  // Explicitly include the Users collection
                            .FirstOrDefaultAsync(x => x.Id == model.Id);
            if (tournament == null)
            {
                return NotFound();
            }
            //viewden gelen turnuva bilgilerinde bir değişiklik yoksa başlangıç
            bool differentUser = false;
            if (model.Name == tournament.Name && model.TableStart == tournament.TableStart && model.StartDate == tournament.StartDate && model.Place == tournament.Place && model.System == tournament.System && model.Type == tournament.Type && model.ByeType == tournament.ByeType && model.PlayLife == tournament.PlayLife)
            {
                foreach (var userId in selectedUsers)
                {
                    var user = await _userManager.FindByIdAsync(userId.ToString());

                    // Add user to AppUserTournament if not already present
                    if (!tournament.Users.Any(u => u.Id == user.Id))
                    {
                        differentUser = true;
                        break;
                    }

                }

                if (!differentUser)
                {
                    TempData["ErrorMessage"] = "evcut bilgilerde bir değişiklik yapmadınız.";
                    return RedirectToAction("UpdateT", new { tournamentId = model.Id });
                }
            }
            //viewden gelen turnuva bilgilerinde bir değişiklik yoksa bitiş
            tournament.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase((model.Name ?? "").ToLower());
            tournament.StartDate = model.StartDate;
            tournament.Place = CultureInfo.CurrentCulture.TextInfo.ToTitleCase((model.Place ?? "").ToLower());
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
                    SMSSendTournamentRegister(user, model.Name);
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
                    TempData["SuccessMessage"] = "Oyuncu başarıyla silindi!";
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, e.Message);
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Kullanıcı veya turnuva bulunamadı";
            }

            return RedirectToAction("UpdateT", new { tournamentId = tournamentId });
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

            ViewBag.TournamentId= tournamentId;
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

        public void SMSSendTournamentRegister(AppUser user,String tournamentName)
        {
            var tarih = new DateTime();
            var client = new RestClient("https://api.vatansms.net/api/v1/NtoN");

            client.Timeout = -1;

            var request = new RestRequest(Method.POST);

            request.AddHeader("Content-Type", "application/json");
            string tName = "";
            string str = tournamentName;
            string[] words = str.Split(' '); // String'i boşluklardan böler ve kelimeleri bir diziye atar
            string lastWord = words[words.Length - 1]; // Dizinin son elemanını alır

            if (lastWord.Contains("turnuva", StringComparison.OrdinalIgnoreCase))
            {
                tName = tournamentName;
            }
            else
            {
                tName = tournamentName + " Turnuvası";
            }

            // Assuming 'scores' is your list of ScoreViewModel objects
            List<object> phoneMessages = new List<object>();


            phoneMessages.Add(new
            {
                phone = user.PhoneNumber,
                message = $"Sayın {user.Name} {user.SurName}, {tName}'na kaydınız yapılmıştır."
            });



            string jsonBody = JsonConvert.SerializeObject(new
            {
                api_id = "5d4219e62fe4475a4585ddea",
                api_key = "e0e87e74a44009c74bf6f4b5",
                sender = "NEFISEVURUR",
                message_type = "turkce",
                message_content_type = "bilgi",
                phones = phoneMessages
            });

            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);

            RestResponse response = (RestResponse)client.Execute(request);

        }


        public void SMSSendSaveScore(Pair pair,int tournamentId)
        {
            var tarih = new DateTime();
            var client = new RestClient("https://api.vatansms.net/api/v1/NtoN");

            client.Timeout = -1;

            var request = new RestRequest(Method.POST);

            request.AddHeader("Content-Type", "application/json");
           

            // Assuming 'scores' is your list of ScoreViewModel objects
            List<object> phoneMessages = new List<object>();
            var user1 = _context.Users.Where(u => u.Id == pair.User1Id && u.Tournaments.Any(t => t.Id == tournamentId)).FirstOrDefault();           
            var user2 = _context.Users.Where(u => u.Id == pair.User2Id && u.Tournaments.Any(t => t.Id == tournamentId)).FirstOrDefault(); ;
            int tourCount = _context.Tournaments
                        .Where(t => t.Id == tournamentId)
                        .Select(t => t.Tours.Count)
                        .FirstOrDefault();
            phoneMessages.Add(new
            {
                phone = user1.PhoneNumber,
                message = $"Tur {tourCount}. {user1.Name} {user1.SurName}: {pair.User1Score} - {pair.User2Score}:{user2.Name} {user2.SurName}"
            });
            phoneMessages.Add(new
            {
                phone = user2.PhoneNumber,
                message = $"Tur {tourCount}. {user1.Name} {user1.SurName}: {pair.User1Score} - {pair.User2Score}:{user2.Name} {user2.SurName}"
            });


            string jsonBody = JsonConvert.SerializeObject(new
            {
                api_id = "5d4219e62fe4475a4585ddea",
                api_key = "e0e87e74a44009c74bf6f4b5",
                sender = "NEFISEVURUR",
                message_type = "turkce",
                message_content_type = "bilgi",
                phones = phoneMessages
            });

            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);

            RestResponse response = (RestResponse)client.Execute(request);

        }



        public IActionResult SMSSend(int tourCount)
        {
            //Retrieve scores JSON string from session
           var scoresJson = HttpContext.Session.GetString("ScoresData");

            if (!string.IsNullOrEmpty(scoresJson))
            {
                // Deserialize scores JSON string back to List<ScoreViewModel>
                var scores = JsonConvert.DeserializeObject<List<ScoreViewModel>>(scoresJson);

                var tarih = new DateTime();
                var client = new RestClient("https://api.vatansms.net/api/v1/NtoN");

                client.Timeout = -1;

                var request = new RestRequest(Method.POST);

                request.AddHeader("Content-Type", "application/json");

                // Assuming 'scores' is your list of ScoreViewModel objects
                List<object> phoneMessages = new List<object>();

                for (int i = 0; i<scores.Count() ;i++)
                {
                   
                    phoneMessages.Add(new
                    {
                        phone = scores[i].User1PhoneNumber,
                        message = $"{tourCount}.Tur başlıyor. {scores[i].User1Name} - {scores[i].User2Name??"bye"}." + (scores[i].User2Name != null ? $"MasaNo: {i + 1}. " : "")
               + "İyi oyunlar."
                    });
                    phoneMessages.Add(new
                    {
                        phone = scores[i].User2PhoneNumber,
                        message = $"{tourCount}.Tur başlıyor. {scores[i].User1Name} - {scores[i].User2Name??"bye"}. MasaNo:{i + 1}. İyi oyunlar."
                    });
                }

                string jsonBody = JsonConvert.SerializeObject(new
                {
                    api_id = "5d4219e62fe4475a4585ddea",
                    api_key = "e0e87e74a44009c74bf6f4b5",
                    sender = "NEFISEVURUR",
                    message_type = "turkce",
                    message_content_type = "bilgi",
                    phones = phoneMessages
                });

                request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);

                RestResponse response = (RestResponse)client.Execute(request);

                return RedirectToAction("Tours", new { tournamentId = scores[0].TournamentId });

            }
           

            //    // Redirect to another action or return a view
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SMSSendT(int tournamentId)
        {
            string message = Request.Form["message"];
            //Retrieve scores JSON string from session
            var users = _context.Users.Where(u => u.Tournaments.Any(t => t.Id == tournamentId)).ToList();

            if (users != null || users.Any())           
            {
                // Deserialize scores JSON string back to List<ScoreViewModel>
                

                var tarih = new DateTime();
                var client = new RestClient("https://api.vatansms.net/api/v1/NtoN");

                client.Timeout = -1;

                var request = new RestRequest(Method.POST);

                request.AddHeader("Content-Type", "application/json");

                // Assuming 'scores' is your list of ScoreViewModel objects
                List<object> phoneMessages = new List<object>();

                for (int i = 0; i < users.Count(); i++)
                {
                   
                        phoneMessages.Add(new
                        {
                            phone = users[i].PhoneNumber,
                            message = $"{message}"
                        });
                    
                                 
                }

                string jsonBody = JsonConvert.SerializeObject(new
                {
                    api_id = "5d4219e62fe4475a4585ddea",
                    api_key = "e0e87e74a44009c74bf6f4b5",
                    sender = "NEFISEVURUR",
                    message_type = "turkce",
                    message_content_type = "bilgi",
                    phones = phoneMessages
                });

                request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);

                RestResponse response = (RestResponse)client.Execute(request);

                return RedirectToAction("Index", "Home");

            }


            //    // Redirect to another action or return a view
            return RedirectToAction("Index","Home");
        }


        public IActionResult SMSSendG()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SMSSendGPostAsync()
        {
            string message = Request.Form["message"];
            var users = await _userService.GetNonAdminUsersAsync();

            if (users != null || users.Any())
            {
                // Deserialize scores JSON string back to List<ScoreViewModel>


                var tarih = new DateTime();
                var client = new RestClient("https://api.vatansms.net/api/v1/NtoN");

                client.Timeout = -1;

                var request = new RestRequest(Method.POST);

                request.AddHeader("Content-Type", "application/json");

                // Assuming 'scores' is your list of ScoreViewModel objects
                List<object> phoneMessages = new List<object>();

                for (int i = 0; i < users.Count(); i++)
                {

                    phoneMessages.Add(new
                    {
                        phone = users[i].PhoneNumber,
                        message = $"{message}"
                    });


                }

                string jsonBody = JsonConvert.SerializeObject(new
                {
                    api_id = "5d4219e62fe4475a4585ddea",
                    api_key = "e0e87e74a44009c74bf6f4b5",
                    sender = "NEFISEVURUR",
                    message_type = "turkce",
                    message_content_type = "bilgi",
                    phones = phoneMessages
                });

                request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);

                RestResponse response = (RestResponse)client.Execute(request);

                return RedirectToAction("Index", "Home");

            }
            return RedirectToAction("Index", "Home");
        }
    }
}

