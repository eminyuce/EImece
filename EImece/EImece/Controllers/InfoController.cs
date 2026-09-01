using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using EImece.Web.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class InfoController : BaseController
    {
        private readonly IMenuService MenuService;

        public InfoController(
            ISettingService settingService,
            AutoMapper.IMapper mapper,
            IMenuService menuService,
            ILogger<InfoController> logger)
            : base(settingService, mapper, logger)
        {
            MenuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        }

        // GET: Info
        public async Task<ActionResult> Index(string id, string lang = "")
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var eImageLang = CurrentLanguage;
            if (!String.IsNullOrEmpty(lang))
            {
                eImageLang = EnumHelper.GetEnumFromDescription(lang, typeof(EImeceLanguage));
            }
            var page = await MenuService.GetPageByMenuLinkAsync(Constants.INFO_PREFIX + id, eImageLang);
            if (page == null)
            {
                page = await MenuService.GetPageByMenuLinkAsync(id, eImageLang);
            }
            if (page == null)
            {
                var settingVal = SettingService.GetSettingByKey(id);
                var allSettings = SettingService.GetAllActiveSettings();
                var settingsList = allSettings != null ? allSettings.Select(s => new SettingKeyValueDto
                {
                    SettingKey = s.SettingKey,
                    SettingValue = s.SettingValue
                }).ToList() : new System.Collections.Generic.List<SettingKeyValueDto>();

                string title = GetFriendlyInfoTitle(id);
                string description = !string.IsNullOrWhiteSpace(settingVal)
                    ? settingVal
                    : GetFriendlyInfoDefaultContent(id, title);

                if (!string.IsNullOrEmpty(title) || !string.IsNullOrWhiteSpace(settingVal))
                {
                    page = new MenuPageViewModel
                    {
                        Menu = new StorefrontMenuDto
                        {
                            Name = !string.IsNullOrEmpty(title) ? title : id,
                            Description = description,
                            IsActive = true
                        },
                        ApplicationSettings = settingsList
                    };
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
            }
            return View(page);
        }

        private static string GetFriendlyInfoTitle(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return string.Empty;
            var normalized = slug.Trim().ToLowerInvariant().Replace("-", "").Replace("_", "");

            switch (normalized)
            {
                case "contactus":
                case "iletisim":
                    return "İletişim";
                case "aboutus":
                case "hakkimizda":
                    return "Hakkımızda";
                case "delivery":
                case "teslimat":
                case "kargo":
                    return "Teslimat ve Kargo Bilgileri";
                case "faq":
                case "sss":
                    return "Sıkça Sorulan Sorular";
                case "privacy":
                case "gizlilik":
                case "gizlilikpolitikasi":
                    return "Gizlilik Politikası";
                case "returnconditions":
                case "iade":
                case "iadevedegisim":
                    return "İade ve Değişim Koşulları";
                case "distancecontract":
                case "mesafelisatis":
                case "mesafelisatissozlesmesi":
                    return "Mesafeli Satış Sözleşmesi";
                case "kvkk":
                case "kvkkaydinlatmametni":
                    return "KVKK Aydınlatma Metni";
                case "terms":
                case "kullanimkosullari":
                    return "Kullanım Koşulları";
                default:
                    return GeneralHelper.GetStringTitleCase(slug.Replace("-", " ").Replace("_", " "));
            }
        }

        private static string GetFriendlyInfoDefaultContent(string slug, string title)
        {
            return $"<div class=\"info-content\"><p>{title} sayfamız güncellenmektedir. Detaylı bilgi için müşteri hizmetlerimizle iletişime geçebilirsiniz.</p></div>";
        }
    }
}