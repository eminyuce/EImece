using EImece.Domain.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class ImageUtilitiesTests
    {
        [TestMethod]
        public void GetPngDimensionsTest()
        {
            string url = "http://dev.magazines.marinelink.com/nwm/MarineTechnologyWhitePapers/201707/content/medium/pageddfdfdf1.jpg";
            var actual = ImageUtilities.GetImageDimensions(url);
            Console.WriteLine(actual.Item1 + "  "+ actual.Item2);
        }
        
        [TestMethod]
        public void GetJpgDimensionsTest()
        {
            string url = "https://upload.wikimedia.org/wikipedia/commons/e/e0/JPEG_example_JPG_RIP_050.jpg";
            Uri uri = new Uri(url);
            var actual = ImageUtilities.GetWebDimensions(uri);
            Assert.AreEqual(new Size(313, 234), actual);
        }

        [TestMethod]
        public void GetGifDimensionsTest()
        {
            string url = "https://upload.wikimedia.org/wikipedia/commons/a/a0/Sunflower_as_gif_websafe.gif";
            Uri uri = new Uri(url);
            var actual = ImageUtilities.GetWebDimensions(uri);
            Assert.AreEqual(new Size(250, 297), actual);
        }
    }
}
