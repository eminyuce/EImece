using EImece.Domain.Services;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class AjaxController : BaseController
    {
        private AdresService adresService { get; set; }

        public AjaxController(AdresService adresService)
        {
            this.adresService = adresService;
        }
        // GET: Ajax
        [HttpGet]
        public async Task<JsonResult> GetTurkiyeIller()
        {
            return await Task.Run(() =>
            {
                return Json(adresService.GetTurkiyeAdres().IlRoot, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }
    }
}