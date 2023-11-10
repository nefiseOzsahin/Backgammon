using Backgammon.Business;
using Backgammon.Entities;
using Backgammon.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


namespace Backgammon.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserService _userService;



        public UserController(IHttpClientFactory clientFactory, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> signInManager, UserService userService)
        {
            _clientFactory = clientFactory;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SignOut()
        {

            await _signInManager.SignOutAsync();
            return RedirectToAction("Index","Home");
        }

        public IActionResult SignIn()
        {
            return View(new AdminSignInModel());
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(AdminSignInModel model)
        {
            if (ModelState.IsValid)
            {
             var result= await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, true);
                if(result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");   

                }else if(result.IsLockedOut)
                {

                }else if(result.IsNotAllowed)
                {

                }

            }
            if (!ModelState.IsValid)
            {
                // Log or debug the validation errors
                foreach (var key in ModelState.Keys)
                {
                    var entry = ModelState[key];
                    foreach (var error in entry.Errors)
                    {
                        // Log or print the error messages
                        // Example: Console.WriteLine($"{key}: {error.ErrorMessage}");
                    }
                }

                // You can also redirect to the same view to display validation errors
                return View(model);
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult CreateAdmin()
         {
           return View(new AdminCreateViewModel());
          }

        [HttpPost]
        public async Task<IActionResult> CreateAdminAsync(AdminCreateViewModel model)
        {

            if (ModelState.IsValid)
            {
                

                AppUser user = new()
                {
                    UserName = model.Username,
                    CreateDate = DateTime.Now
                 
                };

                var result = await _userManager.CreateAsync(user,model.Password);
                if (result.Succeeded)
                {
                    await _roleManager.CreateAsync(new()
                    {
                        Name = "Admin",
                        CreatedTime = DateTime.Now
                    });

                    await _userManager.AddToRoleAsync(user, "Admin");
                    return RedirectToAction("Index");
                }
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }

            }

            if (!ModelState.IsValid)
            {
                // Log or debug the validation errors
                foreach (var key in ModelState.Keys)
                {
                    var entry = ModelState[key];
                    foreach (var error in entry.Errors)
                    {
                        // Log or print the error messages
                        // Example: Console.WriteLine($"{key}: {error.ErrorMessage}");
                    }
                }

                // You can also redirect to the same view to display validation errors
                return View(model);
            }

            return View(model);

        }



        [Authorize(Roles ="Admin")]
        public IActionResult Create()
        {

            return View(new UserCreateViewModel());
            
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(UserCreateViewModel model)
        {          

            if (ModelState.IsValid)
            {
                AppUser user = new()
                {
                    UserName= model.Email,
                    Name = model.Name,
                    SurName = model.Surname,
                    Gender = model.Gender,
                    Email = model.Email,
                    PhoneNumber=model.Phone,
                    ImagePath = model.ImagePath,
                    Club=model.Club,
                    Country=model.Country,
                    City=model.City,
                    IsActive=model.IsActive,
                    CreateDate=DateTime.Now
                };
              
                var result=await _userManager.CreateAsync(user);
                if(result.Succeeded)
                {                  
                    return RedirectToAction("Index");
                }                
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }

            }

            if (!ModelState.IsValid)
            {
                // Log or debug the validation errors
                foreach (var key in ModelState.Keys)
                {
                    var entry = ModelState[key];
                    foreach (var error in entry.Errors)
                    {
                        // Log or print the error messages
                        // Example: Console.WriteLine($"{key}: {error.ErrorMessage}");
                    }
                }

                // You can also redirect to the same view to display validation errors
                return View(model);
            }

            return View(model);

        }

        public async Task<IActionResult> GetCity(string countryCode)
        {
            CountryCityModel viewModel = new CountryCityModel();

            try
            {
                var client = _clientFactory.CreateClient();
                var countryResponse = await client.GetAsync("http://api.geonames.org/countryInfoJSON?username=nefise");

                if (countryResponse.IsSuccessStatusCode)
                {
                    var countryJson = await countryResponse.Content.ReadAsStringAsync();
                    var countryInfo = JsonSerializer.Deserialize<CountryInfo>(countryJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    viewModel.CountryList = countryInfo.Geonames;
                }
                else
                {
                    ViewBag.ErrorMessage = "Error from GeoNames API when fetching countries.";
                }

                if (!string.IsNullOrWhiteSpace(countryCode))
                {
                    var endpoint = $"http://api.geonames.org/searchJSON?country={countryCode}&username=nefise";
                    var response = await client.GetAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var cityList = JsonSerializer.Deserialize<CityInfo>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        viewModel.CityList = cityList.Geonames;
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Error from GeoNames API when fetching cities.";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            return Json(viewModel);
           
        }
    
    
        public async Task<IActionResult> GetUserListAsync()
        {
            var nonAdminUsers = await _userService.GetNonAdminUsersAsync();
            return View(nonAdminUsers);
        }

        public async Task<IActionResult> UpdateUserAsync(int userId)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(x => x.Id == userId);

            return View(user);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateUserAsync(AppUser model, bool IsActive)
        {
            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user == null)
            {
                return NotFound();
            }
            await _userManager.UpdateSecurityStampAsync(user);

            user.UserName = model.Email;
            user.Name = model.Name ?? "";
            user.SurName = model.SurName ?? "";
            user.Email = model.Email;
            user.Gender = model.Gender;
            user.PhoneNumber = model.PhoneNumber;       
            user.ImagePath = model.ImagePath;
            user.Club=model.Club;
            user.Country = model.Country;
            user.City = model.City;
            user.IsActive = model.IsActive;
         


            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }

            // Update successful, redirect or return success message
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {

                    return RedirectToAction("Index");
                }
                else
                {
                    return NotFound();
                }
            }

            return View("Index");
        }

    }
}
