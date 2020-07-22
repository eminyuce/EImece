using EImece.Domain;
using EImece.Domain.Helpers.AttributeHelper;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Ninject;
using System.Web.Mvc;

namespace EImece.Areas.Customers.Controllers
{
    [AuthorizeRoles(Domain.Constants.CustomerRole)]
    public class HomeController : Controller
    {
        [Inject]
        public IAuthenticationManager AuthenticationManager { get; set; }

        // GET: Customers/Home
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home", new { @area = "" });
        }
    }
}