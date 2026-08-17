using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [RoutePrefix(Constants.PagesControllerRoutingPrefix)]
    public class PagesController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IMenuService MenuService { get; set; }

        // Old hashed contact slugs (hash changes when the CMS page is re-saved).
        private static readonly HashSet<string> LegacyContactSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "iletisim-3f4h8c6g",
            "iletisim-4h5i8c6g"
        };

        [CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
        public async Task<ActionResult> Detail(String id = "")
        {
            try
            {
                if (String.IsNullOrEmpty(id))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var legacyRedirect = await TryRedirectLegacyContactAsync(id).ConfigureAwait(false);
                if (legacyRedirect != null)
                {
                    return legacyRedirect;
                }
                var menuId = id.GetId();
                var page = await MenuService.GetPageByIdAsync(menuId);
                if (page == null || page.Menu == null)
                {
                    Logger.Warn("Pages/Detail: menu not found for id '{0}' (parsed {1}).", id, menuId);
                    return RedirectToAction("NotFound", "Error");
                }

                ViewBag.SeoId = page.Menu.GetSeoUrl();
                if (page.Menu.IsActive)
                {
                    return View(page);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                return HandleUnexpectedError(ex, ex.Message + " id:" + id);
            }
        }

        private async Task<ActionResult> TryRedirectLegacyContactAsync(string id)
        {
            var slug = (id ?? string.Empty).Trim().Trim('/');
            if (!LegacyContactSlugs.Contains(slug))
            {
                return null;
            }

            var menus = await MenuService.GetMenusAsync().ConfigureAwait(false);
            var contact = menus == null
                ? null
                : menus.FirstOrDefault(m => m != null
                    && m.IsActive
                    && m.GetSeoUrl().StartsWith("iletisim-", StringComparison.OrdinalIgnoreCase));

            if (contact != null)
            {
                var canonical = Url.Action("Detail", "Pages", new { id = contact.GetSeoUrl() });
                if (!string.IsNullOrEmpty(canonical) && !canonical.Trim('/').EndsWith(slug, StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectPermanent(canonical);
                }
            }

            return RedirectPermanent("/i/iletisim-1b9a2d6g/");
        }
    }
}