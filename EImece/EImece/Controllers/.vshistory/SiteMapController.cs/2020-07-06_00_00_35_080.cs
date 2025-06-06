﻿using EImece.Domain;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.SiteMap;
using EImece.Domain.Services;
using Ninject;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class SiteMapController : BaseController
    {
        [Inject]
        public SiteMapService SiteMapService { get; set; }

        [CustomOutputCache(CacheProfile = Constants.Cache1Hour)]
        [Route("sitemap.xml")]
        [HttpGet]
        public ActionResult Index()
        {
            return new SitemapResult(SiteMapService.GenerateSiteMap());
        }
    }
}