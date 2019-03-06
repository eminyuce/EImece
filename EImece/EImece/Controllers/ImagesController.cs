using EImece.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using EImece.Domain.Helpers.AttributeHelper;

namespace EImece.Controllers
{
    public class ImagesController : BaseController
    {
        // GET: Images
        [AcceptVerbs(HttpVerbs.Get)]
        [CustomOutputCache(CacheProfile = "ImageProxyCaching")]
        public ActionResult Index(String id, String imageSize)
        {
            var fileStorageId = id.Replace(".jpg", "").GetIdWithoutDecode();

            if (fileStorageId > 0)
            {
                int height = 0;
                int width = 0;
                if (String.IsNullOrEmpty(imageSize))
                {
                    imageSize = "w150h150";
                }

                width = Regex.Match(imageSize, @"w(\d*)").Value.Replace("w", "").ToInt();
                height = Regex.Match(imageSize, @"h(\d*)").Value.Replace("h", "").ToInt();

                var imageByte = FilesHelper.GetResizedImage(fileStorageId, width, height);

                if (imageByte !=null && imageByte.Item1 != null)
                {
                    return File(imageByte.Item1, imageByte.Item2);
                }
                else
                {
                    return new EmptyResult();
                }
            }
            else
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
            Image image = Image.FromStream(new MemoryStream(imageByte.Item1));

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

            return File(ms.ToArray(), imageByte.Item2);
        }

        //controller function to generate image
        //returns image file (through url.action in img src attribute)
        public ActionResult GetCaptcha(string prefix, bool noisy = true)
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            //generate new question 
            int a = rand.Next(10, 99);
            int b = rand.Next(0, 9);
            var captcha = string.Format("{0} + {1} = ?", a, b);

            //store answer 
            Session["Captcha" + prefix] = a + b;

            //image stream 
            FileContentResult img = null;

            try
            {
                img = this.File(FilesHelper.GenerateCaptchaImg(captcha, true), "image/Jpeg");
            }
            catch
            {
            }

            return img;
        }

    }
}