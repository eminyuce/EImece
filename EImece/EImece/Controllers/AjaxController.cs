using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Ninject;
using Resources;
using System.Collections.Generic;
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
                return Json(Resource.NotValidEmailAddress, JsonRequestBehavior.AllowGet);
            }

            if (await SubsciberService.GetSubscriberByEmailAsync(subscribeEmail) == null)
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
                await SubsciberService.SaveOrEditEntityAsync(subscriber);
            }
            return Json("success", JsonRequestBehavior.AllowGet);
        }


        [CustomOutputCache(CacheProfile = Constants.Cache30Days)]
        public JsonResult GetAllCities()
        {
            var cities = turkishRegionService.GetAllCities()
                .OrderBy(city => city)
                .Select(city => new SelectListItem
                {
                    Value = city,
                    Text = city
                }).ToList();

            cities.Insert(0, new SelectListItem { Value = "", Text = Resource.Select });

            return Json(cities, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTownsByCity(string cityName)
        {
            var towns = turkishRegionService.GetTownsByCity(cityName)
                .OrderBy(town => town)
                .Select(town => new SelectListItem
                {
                    Value = town,
                    Text = town
                }).ToList();

            towns.Insert(0, new SelectListItem { Value = "", Text = Resource.Select });

            return Json(towns, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDistrictsByTown(string cityName, string townName)
        {
            var districts = turkishRegionService.GetDistrictsByTown(cityName, townName)
                .OrderBy(d => d)
                .Select(d => new SelectListItem
                {
                    Value = d,
                    Text = d
                }).ToList();

            districts.Insert(0, new SelectListItem { Value = "", Text = Resource.Select });

            return Json(districts, JsonRequestBehavior.AllowGet);
        }

        // GET: Ajax
        public JsonResult GetIller()
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
        }

        public JsonResult GetIlceler(int il_id)
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
        }
    }
}
