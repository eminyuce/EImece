using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EImece.Domain.Services
{
    public class TurkishRegionService : ITurkishRegionService
    {
        private static readonly Lazy<List<City>> CachedCities = new Lazy<List<City>>(LoadDataInternal, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        public TurkishRegionService()
        {
        }

        private static List<City> LoadDataInternal()
        {
            var dataDir = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
            string filePath = null;

            if (!string.IsNullOrEmpty(dataDir))
            {
                var candidate = Path.Combine(dataDir, "data.json");
                if (File.Exists(candidate))
                {
                    filePath = candidate;
                }
            }

            if (filePath == null)
            {
                var candidate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "data.json");
                if (File.Exists(candidate))
                {
                    filePath = candidate;
                }
            }

            if (filePath != null && File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<City>>(json) ?? new List<City>();
            }

            return new List<City>();
        }

        public List<string> GetAllCities()
        {
            return CachedCities.Value.Select(c => c.Name).ToList();
        }

        public List<string> GetTownsByCity(string cityName)
        {
            var city = CachedCities.Value.FirstOrDefault(c => c.Name.Equals(cityName, StringComparison.OrdinalIgnoreCase));
            if (city != null)
            {
                return city.Towns.Select(t => t.Name).ToList();
            }
            return new List<string>();
        }

        public List<string> GetDistrictsByTown(string cityName, string townName)
        {
            var city = CachedCities.Value.FirstOrDefault(c => c.Name.Equals(cityName, StringComparison.OrdinalIgnoreCase));
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