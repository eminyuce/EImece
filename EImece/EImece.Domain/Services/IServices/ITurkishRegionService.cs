using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface ITurkishRegionService
    {
        List<string> GetAllCities();
        List<string> GetTownsByCity(string cityName);
        List<string> GetDistrictsByTown(string cityName, string townName);
    }
}
