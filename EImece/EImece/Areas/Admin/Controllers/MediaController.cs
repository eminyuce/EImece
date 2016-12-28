using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EImece.Domain.Entities;
using EImece.Domain.Models.AdminModels;

namespace EImece.Areas.Admin.Controllers
{
    public class MediaController : BaseAdminController
    {
        // GET: Admin/Media
        public ActionResult Index(int id, String mod, String imageType)
        {
            var returnModel = new MediaAdminIndexModel();
            MediaModType ? enumMod = EnumHelper.Parse<MediaModType>(mod, true);
            EImeceImageType? enumImageType = EnumHelper.Parse<EImeceImageType>(imageType, true);
            returnModel.Id = id;
            returnModel.imageType = enumImageType.Value;
            returnModel.mediaMod = enumMod.Value;

            switch (enumMod)
            {
                case MediaModType.Stories:
                    returnModel.baseContent = StoryService.GetSingle(id);
                    break;
                case MediaModType.Products:
                    returnModel.baseContent = ProductService.GetSingle(id);
                    break;

                default:
                    break;
            }


            return View(returnModel);
        }
    }
}