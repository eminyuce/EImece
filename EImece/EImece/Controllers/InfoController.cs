using EImece.Domain.Services.IServices;
using Ninject;
using System.Net;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class InfoController : BaseController
    {
        [Inject]
        public IMenuService MenuService { get; set; }

        // GET: Info
        public ActionResult Index(string id)
        {
            var page = MenuService.GetPageByMenuLink("info-" + id, CurrentLanguage);
            if (page == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            return View(page);
        }
    }
}