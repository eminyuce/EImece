using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Ninject;
using Resources;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class AjaxController : BaseController
    {
        private AdresService adresService { get; set; }

        [Inject]
        public IOrderService OrderService { get; set; }

        [Inject]
        public ISubscriberService SubsciberService { get; set; }

        private const string Main_Page_Product_Subscription = "Main-Page-Product-Subscription";
        public TurkishRegionService turkishRegionService;

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
            turkishRegionService = new TurkishRegionService();
        }

        public async Task<JsonResult> SubscribeEmail(string subscribeEmail)
        {
            if (GeneralHelper.IsNotValidEmail(subscribeEmail))
            {
                return await Task.Run(() =>
                {
                    return Json(Resource.NotValidEmailAddress, JsonRequestBehavior.AllowGet);  // Return the list directly
                }).ConfigureAwait(true);
            }
            else
            {
                return await Task.Run(() =>
                {
                    if (SubsciberService.GetSubscriberByEmail(subscribeEmail) == null)
                    {
                        var subscriber = new Subscriber();
                        subscriber.Name = subscribeEmail;
                        subscriber.Email = subscribeEmail;
                        subscriber.Note = Main_Page_Product_Subscription;
                        subscriber.IsActive = true;
                        subscriber.CreatedDate = System.DateTime.Now;
                        subscriber.UpdatedDate = System.DateTime.Now;
                        subscriber.Position = 1;
                        subscriber.Lang = CurrentLanguage;
                        SubsciberService.SaveOrEditEntity(subscriber);
                    }
                    return Json("success", JsonRequestBehavior.AllowGet);  // Return the list directly
                }).ConfigureAwait(true);
            }
        }


        [CustomOutputCache(CacheProfile = Constants.Cache30Days)]
        public async Task<JsonResult> GetAllCities()
        {
            return await Task.Run(() =>
            {
                var allIller = turkishRegionService.GetAllCities().OrderBy(r => r).ToList();
                return Json(allIller, JsonRequestBehavior.AllowGet);  // Return the list directly
            }).ConfigureAwait(true);
        }

        public async Task<JsonResult> GetTownsByCity(string cityName)
        {
            return await Task.Run(() =>
            {
                var ilceler = turkishRegionService.GetTownsByCity(cityName).OrderBy(r => r).ToList();

                return Json(ilceler, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        public async Task<JsonResult> GetDistrictsByTown(string cityName, string townName)
        {
            return await Task.Run(() =>
            {
                var districts = turkishRegionService.GetDistrictsByTown(cityName, townName).OrderBy(r => r).ToList();

                return Json(districts, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
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