using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public class ImagesController : BaseController
    {
        // GET: Images
        [AcceptVerbs(HttpVerbs.Get)]
        //[OutputCache(CacheProfile = "CustomerImages")]
        public ActionResult Index(int id, int width, int height)
        {
            var fileStorageId = id;
            var imageByte = FilesHelper.GetResizedImage(fileStorageId, width, height);
            if(imageByte != null)
            {
                return File(imageByte, "image/jpg");
            }else
            {
                return new EmptyResult();
            }

        }
        public ActionResult GetModifiedImage()
        {
            Image image = Image.FromFile(Path.Combine(Server.MapPath("/Content/images"), "image.png"));

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

            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            return File(ms.ToArray(), "image/png");
        }
    }
}