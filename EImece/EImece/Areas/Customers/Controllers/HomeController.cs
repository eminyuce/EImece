using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using EImece.Models;
using Ninject;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Customers.Controllers
{
    [AuthorizeRoles(Constants.CustomerRole)]
    public class HomeController : Controller
    {
        // GET: Customers/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}