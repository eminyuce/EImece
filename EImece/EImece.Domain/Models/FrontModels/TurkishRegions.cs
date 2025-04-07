using Newtonsoft.Json;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    internal class TurkishRegions
    {
    }

    public class City
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("alpha_2_code")]
        public string Alpha2Code { get; set; }

        [JsonProperty("towns")]
        public List<Town> Towns { get; set; }
    }

    public class Town
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("districts")]
        public List<District> Districts { get; set; }
    }

    public class District
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("quarters")]
        public List<Quarter> Quarters { get; set; }
    }

    public class Quarter
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}