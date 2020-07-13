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
        public async Task<JsonResult> GetIller()
        {
            return await Task.Run(() => Json(adresService.GetTurkiyeAdres().IlRoot, JsonRequestBehavior.AllowGet)).ConfigureAwait(true);
        }
        [HttpGet]
        public async Task<JsonResult> GetIlceler(int ilId)
        {
            return await Task.Run(() => Json(adresService.GetTurkiyeAdres().IlceRoot.ilceler.ilce.FindAll(r => r.il_id == ilId), JsonRequestBehavior.AllowGet)).ConfigureAwait(true);
        }
       
    }
}