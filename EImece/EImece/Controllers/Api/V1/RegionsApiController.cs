using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using System.Linq;
using System.Web.Http;

namespace EImece.Controllers.Api.V1
{
    /// <summary>
    /// Regional lookup endpoints for Turkish cities, towns, and districts.
    /// </summary>
    [RoutePrefix("api/v1/regions")]
    public class RegionsApiController : ApiController
    {
        private readonly ITurkishRegionService _turkishRegionService;

        public RegionsApiController(ITurkishRegionService turkishRegionService)
        {
            _turkishRegionService = turkishRegionService ?? new TurkishRegionService();
        }

        public RegionsApiController()
            : this(null)
        {
        }

        /// <summary>
        /// Gets list of all Turkish cities.
        /// </summary>
        [HttpGet]
        [Route("cities")]
        public IHttpActionResult GetCities()
        {
            var cities = _turkishRegionService.GetAllCities()
                .OrderBy(c => c)
                .Select(c => new { Name = c })
                .ToList();

            return Ok(cities);
        }

        /// <summary>
        /// Gets towns for a specified city.
        /// </summary>
        [HttpGet]
        [Route("cities/{cityName}/towns")]
        public IHttpActionResult GetTowns(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return BadRequest("City name cannot be empty.");

            var towns = _turkishRegionService.GetTownsByCity(cityName)
                .OrderBy(t => t)
                .Select(t => new { Name = t })
                .ToList();

            return Ok(towns);
        }

        /// <summary>
        /// Gets districts for a specified city and town.
        /// </summary>
        [HttpGet]
        [Route("cities/{cityName}/towns/{townName}/districts")]
        public IHttpActionResult GetDistricts(string cityName, string townName)
        {
            if (string.IsNullOrWhiteSpace(cityName) || string.IsNullOrWhiteSpace(townName))
                return BadRequest("City and Town name cannot be empty.");

            var districts = _turkishRegionService.GetDistrictsByTown(cityName, townName)
                .OrderBy(d => d)
                .Select(d => new { Name = d })
                .ToList();

            return Ok(districts);
        }
    }
}
