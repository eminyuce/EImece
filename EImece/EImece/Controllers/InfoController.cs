using EImece.Domain;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using System;
using System.Net;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class InfoController : BaseController
    {
        private readonly IMenuService _menuService;

        public InfoController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        // GET: Info
        public ActionResult Index(string id, string lang = "")
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
            var page = _menuService.GetPageByMenuLink(Constants.INFO_PREFIX + id, eImageLang);
            if (page == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            return View(page);
        }
    }
}
