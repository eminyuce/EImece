using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Helpers.SiteMap;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace EImece.Controllers
{
    public class SiteMapController : BaseController
    {
        [Inject]
        public SiteMapService SiteMapService { get; set; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [CustomOutputCache(CacheProfile = "Cache1Hour")]
        [Route("sitemap.xml")]
        public ActionResult Index()
        {
            return new SitemapResult(SiteMapService.GenerateSiteMap());
        }


    }
}