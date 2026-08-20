using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace EImece.Domain.Services
{
    public class TurkishRegionService : ITurkishRegionService
    {
        private List<City> _cities;

        public TurkishRegionService()
        {
            // Initialize and load data from the file
            LoadData();
        }

        // Method to read the file and cache it
        private void LoadData()
        {
            // Specify the file path where your JSON is located
            string filePath = null;
            if (HostingEnvironment.IsHosted)
            {
                filePath = HostingEnvironment.MapPath("~/App_Data/data.json");
            }
            else if (HttpContext.Current != null && HttpContext.Current.Server != null)
            {
                filePath = HttpContext.Current.Server.MapPath("~/App_Data/data.json");
            }
            else
            {
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "data.json");
            }

            if (filePath != null && File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                _cities = JsonConvert.DeserializeObject<List<City>>(json) ?? new List<City>();
            }
            else
            {
                _cities = new List<City>();
            }
        }

        // Get all cities
        public List<string> GetAllCities()
        {
            return _cities.Select(c => c.Name).ToList();
        }

        // Get all towns based on selected city
        public List<string> GetTownsByCity(string cityName)
        {
            var city = _cities.FirstOrDefault(c => c.Name.Equals(cityName, StringComparison.OrdinalIgnoreCase));

            if (city != null)
            {
                return city.Towns.Select(t => t.Name).ToList();
            }

            return new List<string>();
        }

        // Get all districts based on selected town
        public List<string> GetDistrictsByTown(string cityName, string townName)
        {
            var city = _cities.FirstOrDefault(c => c.Name.Equals(cityName, StringComparison.OrdinalIgnoreCase));
            if (city != null)
            {
                var town = city.Towns.FirstOrDefault(t => t.Name.Equals(townName, StringComparison.OrdinalIgnoreCase));
                if (town != null)
                {
                    return town.Districts
                               .SelectMany(d => d.Quarters)
                               .Select(q => q.Name)
                               .OrderBy(q => q)
                               .ToList();
                }
            }

            return new List<string>();
        }
    }
}