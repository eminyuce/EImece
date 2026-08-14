using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IWebAppManifestService
    {
        /// <summary>
        /// Returns cached UTF-8 Web App Manifest JSON branded from site settings.
        /// </summary>
        Task<string> GetManifestJsonAsync();
    }
}
