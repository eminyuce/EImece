﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class ImagesController : BaseAdminController
    {
        [AcceptVerbs(HttpVerbs.Get)]
        //[OutputCache(CacheProfile = "CustomerImages")]
        public ActionResult Index(int id, int width=0, int height=0)
        {
            var fileStorageId = id;
            var imageByte = FilesHelper.GetResizedImage(fileStorageId, width, height);
            if (imageByte != null)
            {
                return File(imageByte, "image/jpg");
            }
            else
            {
                return new EmptyResult();
            }

        }
    }
}