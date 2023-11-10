using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;

namespace Backgammon.Models
{
    public class CountryCityModel
    {
     
        public List<Country> CountryList { get; set; }       
        public List<City> CityList { get; set; }

    }

    public class CountryInfo
    {
        public List<Country> Geonames { get; set; }
    }

    public class Country
    {
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
    }

    public class CityInfo
    {
        public List<City> Geonames { get; set; }
    }

    public class City
    {
        public string Name { get; set; }

    }

}
