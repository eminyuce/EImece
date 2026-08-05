using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EImece.Domain.Core.Services;

public interface ITurkishRegionService
{
    IReadOnlyList<string> GetAllCities();
    IReadOnlyList<string> GetTownsByCity(string cityName);
    IReadOnlyList<string> GetDistrictsByTown(string cityName, string townName);
}

public sealed class TurkishRegionService : ITurkishRegionService
{
    private readonly IReadOnlyList<CityNode> _cities;

    public TurkishRegionService(IHostEnvironment environment, ILogger<TurkishRegionService> logger)
    {
        var candidates = new[]
        {
            Path.Combine(environment.ContentRootPath, "App_Data", "data.json"),
            Path.Combine(environment.ContentRootPath, "..", "EImece", "App_Data", "data.json")
        };

        foreach (var path in candidates)
        {
            var full = Path.GetFullPath(path);
            if (!File.Exists(full))
            {
                continue;
            }

            try
            {
                var json = File.ReadAllText(full);
                var cities = JsonSerializer.Deserialize<List<CityNode>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? [];
                _cities = cities;
                logger.LogInformation("Loaded {Count} Turkish cities from {Path}", cities.Count, full);
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to load Turkish regions from {Path}", full);
            }
        }

        logger.LogWarning("Turkish region data.json not found; using empty list");
        _cities = [];
    }

    public IReadOnlyList<string> GetAllCities()
        => _cities.Select(c => c.Name).Where(n => !string.IsNullOrWhiteSpace(n)).OrderBy(n => n).ToList();

    public IReadOnlyList<string> GetTownsByCity(string cityName)
    {
        var city = FindCity(cityName);
        return city?.Towns.Select(t => t.Name).Where(n => !string.IsNullOrWhiteSpace(n)).OrderBy(n => n).ToList()
               ?? (IReadOnlyList<string>)[];
    }

    public IReadOnlyList<string> GetDistrictsByTown(string cityName, string townName)
    {
        var city = FindCity(cityName);
        var town = city?.Towns.FirstOrDefault(t => string.Equals(t.Name, townName, StringComparison.OrdinalIgnoreCase));
        if (town is null)
        {
            return [];
        }

        return town.Districts
            .SelectMany(d => d.Quarters.Select(q => q.Name))
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .OrderBy(n => n)
            .ToList();
    }

    private CityNode? FindCity(string cityName)
        => _cities.FirstOrDefault(c => string.Equals(c.Name, cityName, StringComparison.OrdinalIgnoreCase));

    private sealed class CityNode
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("towns")]
        public List<TownNode> Towns { get; set; } = [];
    }

    private sealed class TownNode
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("districts")]
        public List<DistrictNode> Districts { get; set; } = [];
    }

    private sealed class DistrictNode
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("quarters")]
        public List<QuarterNode> Quarters { get; set; } = [];
    }

    private sealed class QuarterNode
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
