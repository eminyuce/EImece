using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class InfoController : BaseController
    {
        [Inject]
        public IMenuService MenuService { get; set; }

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
                if (!string.IsNullOrWhiteSpace(settingVal))
                {
                    var allSettings = SettingService.GetAllActiveSettings();
                    page = new MenuPageViewModel
                    {
                        Menu = new StorefrontMenuDto
                        {
                            Name = id,
                            Description = settingVal,
                            IsActive = true
                        },
                        ApplicationSettings = allSettings != null ? allSettings.Select(s => new SettingDto
                        {
                            Id = s.Id,
                            SettingKey = s.SettingKey,
                            SettingValue = s.SettingValue,
                            Lang = s.Lang
                        }).ToList() : new System.Collections.Generic.List<SettingDto>()
                    };
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
            }
            return View(page);
        }
    }
}