using System;
using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Filters;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
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
        private readonly IOrderService OrderService;
        private readonly ISubscriberService SubsciberService;
        private readonly ITurkishRegionService TurkishRegionService;

        private const string Main_Page_Product_Subscription = "Main-Page-Product-Subscription";

        public AjaxController(
            ISettingService settingService,
            AutoMapper.IMapper mapper,
            IOrderService orderService,
            ISubscriberService subsciberService,
            ITurkishRegionService turkishRegionService)
            : base(settingService, mapper)
        {
            OrderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            SubsciberService = subsciberService ?? throw new ArgumentNullException(nameof(subsciberService));
            TurkishRegionService = turkishRegionService ?? throw new ArgumentNullException(nameof(turkishRegionService));
        }

        [HttpPost]
        public JsonResult HomePageShoppingCart()
        {
            var tempData = new TempDataDictionary();
            var html = this.RenderPartialToString(
                        "ShoppingCartTemplates/_HomePageShoppingCart",
                        new ViewDataDictionary(), tempData);
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> SubscribeEmail(string subscribeEmail)
        {
            if (GeneralHelper.IsNotValidEmail(subscribeEmail))
            {
                return Json(Resource.NotValidEmailAddress, JsonRequestBehavior.AllowGet);
            }

            if (!await SubsciberService.SubscriberExistsByEmailAsync(subscribeEmail))
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
        public Task<JsonResult> GetAllCities()
        {
            var service = TurkishRegionService ?? new TurkishRegionService();
            var cities = service.GetAllCities()
                .OrderBy(city => city)
                .Select(city => new SelectListItem
                {
                    Value = city,
                    Text = city
                }).ToList();

            cities.Insert(0, new SelectListItem { Value = "", Text = Resource.Select });

            return Task.FromResult(Json(cities, JsonRequestBehavior.AllowGet));
        }

        public Task<JsonResult> GetTownsByCity(string cityName)
        {
            var service = TurkishRegionService ?? new TurkishRegionService();
            var towns = service.GetTownsByCity(cityName)
                .OrderBy(town => town)
                .Select(town => new SelectListItem
                {
                    Value = town,
                    Text = town
                }).ToList();

            towns.Insert(0, new SelectListItem { Value = "", Text = Resource.Select });

            return Task.FromResult(Json(towns, JsonRequestBehavior.AllowGet));
        }

        public Task<JsonResult> GetDistrictsByTown(string cityName, string townName)
        {
            var service = TurkishRegionService ?? new TurkishRegionService();
            var districts = service.GetDistrictsByTown(cityName, townName)
                .OrderBy(d => d)
                .Select(d => new SelectListItem
                {
                    Value = d,
                    Text = d
                }).ToList();

            districts.Insert(0, new SelectListItem { Value = "", Text = Resource.Select });

            return Task.FromResult(Json(districts, JsonRequestBehavior.AllowGet));
        }
    }
}
