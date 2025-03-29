using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Net;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class InfoController : BaseController
    {
        [Inject]
        public IMenuService MenuService { get; set; }

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
            var page = MenuService.GetPageByMenuLink(Constants.INFO_PREFIX + id, eImageLang);
            if (page == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            return View(page);
        }
    }
}