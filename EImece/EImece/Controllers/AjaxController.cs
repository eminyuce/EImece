using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class AjaxController : BaseController
    {
        // GET: Ajax
        public ActionResult Index()
        {
            return View();
        }
    }
}