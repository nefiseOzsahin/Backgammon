using Backgammon.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Backgammon.Controllers
{
    public class CountryController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public CountryController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.GetAsync("http://api.geonames.org/countryInfoJSON?username=nefise");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var countryInfo = System.Text.Json.JsonSerializer.Deserialize<CountryInfo>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return Json(countryInfo.Geonames);
                }
                else
                {
                    // Handle the error response
                    ViewBag.ErrorMessage = "Error from GeoNames API";
                    return View();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, e.g., network errors
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }
    }
}
