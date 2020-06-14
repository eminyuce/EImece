using System;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Models.HelperModels
{
    public class ViewDataUploadFilesResult
    {
        public string name { get; set; }
        public int size { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public string deleteUrl { get; set; }
        public string thumbnailUrl { get; set; }
        public string deleteType { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public String mimeType { get; set; }
        public int fileStorageId { get; set; }
        public string imageHash { get; set; }

        public String fileImageSrc
        {
            get
            {
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                var imageId = String.Format("{0}.jpg", fileStorageId);
                String imagePath = urlHelper.Action("Index", "Images", new { area = "admin", id = imageId, width = 150, height = 0 });
                return imagePath;
            }
        }
    }
}