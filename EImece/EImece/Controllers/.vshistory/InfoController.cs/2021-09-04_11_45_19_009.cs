using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class InfoController : BaseController
    {
        [Inject]
        public IMenuService MenuService { get; set; }

        // GET: Info
        public ActionResult Index(string id, string lang)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var eImageLang = EnumHelper.GetEnumFromDescription(lang, typeof(EImeceLanguage));
            var page = MenuService.GetPageByMenuLink(Constants.INFO_PREFIX + id, eImageLang);
            if (page == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            return View(page);
        }
    }
}