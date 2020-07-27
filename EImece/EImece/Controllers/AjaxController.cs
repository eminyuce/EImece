using EImece.Domain.Helpers;
using EImece.Domain.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class AjaxController : BaseController
    {
        private AdresService adresService { get; set; }

        [HttpPost]
        public JsonResult HomePageShoppingCart()
        {
            var tempData = new TempDataDictionary();
            var html = this.RenderPartialToString(
                        @"~\Views\Shared\ShoppingCartTemplates\_HomePageShoppingCart.cshtml",
                        new ViewDataDictionary(), tempData);
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        public AjaxController(AdresService adresService)
        {
            this.adresService = adresService;
        }

        // GET: Ajax
        public async Task<JsonResult> GetIller()
        {
            return await Task.Run(() =>
            {
                var allIller = from cust in adresService.GetTurkiyeAdres().IlRoot.Iller.il
                               select new
                               {
                                   id = cust.id,
                                   name = cust.il_adi
                               };

                return Json(
                    new
                    {
                        allIller
                    }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        public async Task<JsonResult> GetIlceler(int il_id)
        {
            return await Task.Run(() =>
            {
                var allIceler = from cust in adresService.GetTurkiyeAdres().IlceRoot.ilceler.ilce
                                where cust.il_id == il_id
                                select new
                                {
                                    id = cust.id,
                                    name = cust.ilce_adi
                                };

                return Json(
                    new
                    {
                        items = allIceler
                    }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }
    }
}