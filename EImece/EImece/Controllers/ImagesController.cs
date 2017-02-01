using EImece.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class ImagesController : BaseController
    {
        // GET: Images
        [AcceptVerbs(HttpVerbs.Get)]
        //[OutputCache(CacheProfile = "CustomerImages")]
        public ActionResult Index(String id, String imageSize)
        {
            int height = 0;
            int width = 0;
            if (String.IsNullOrEmpty(imageSize))
            {
                imageSize = "w150h150";
            }

            width = Regex.Match(imageSize, @"w(\d*)").Value.Replace("w","").ToInt();
            height = Regex.Match(imageSize, @"h(\d*)").Value.Replace("h", "").ToInt();

            var fileStorageId = id.Replace(".jpg", "").GetId();
            var imageByte = FilesHelper.GetResizedImage(fileStorageId, width, height);
            if(imageByte != null)
            {
                return File(imageByte, "image/jpg");
            }else
            {
                return new EmptyResult();
            }

        }
        public ActionResult GetModifiedImage(String id, String imageSize)
        {
            int height = 0;
            int width = 0;
            if (String.IsNullOrEmpty(imageSize))
            {
                imageSize = "w150h150";
            }

            width = Regex.Match(imageSize, @"w(\d*)").Value.Replace("w", "").ToInt();
            height = Regex.Match(imageSize, @"h(\d*)").Value.Replace("h", "").ToInt();

            var fileStorageId = id.Replace(".jpg", "").ToInt();
            var imageByte = FilesHelper.GetResizedImage(fileStorageId, width, height);
            Image image = Image.FromStream(new MemoryStream(imageByte));

            using (Graphics g = Graphics.FromImage(image))
            {
                // do something with the Graphics (eg. write "Hello World!")
                string text = "Hello World!";

                // Create font and brush.
                Font drawFont = new Font("Arial", 10);
                SolidBrush drawBrush = new SolidBrush(Color.Black);

                // Create point for upper-left corner of drawing.
                PointF stringPoint = new PointF(0, 0);

                g.DrawString(text, drawFont, drawBrush, stringPoint);
            }

            MemoryStream ms = new MemoryStream();

            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

            return File(ms.ToArray(), "image/jpeg");
        }
    }
}