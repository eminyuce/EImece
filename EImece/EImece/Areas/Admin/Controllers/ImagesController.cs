using EImece.Domain.Helpers;
using System;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class ImagesController : BaseAdminController
    {
        [AcceptVerbs(HttpVerbs.Get)]
        //[CustomOutputCache(CacheProfile = "CustomerImages")]
        public ActionResult Index(String id, int width = 0, int height = 0)
        {
            var fileStorageId = id.Replace(".jpg", "").ToInt();
            var imageByte = FilesHelper.GetResizedImage(fileStorageId, width, height);
            if (imageByte != null)
            {
                return File(imageByte.ImageBytes, imageByte.ContentType);
            }
            else
            {
                return new EmptyResult();
            }
        }
    }
}