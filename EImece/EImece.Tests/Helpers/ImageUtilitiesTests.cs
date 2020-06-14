using EImece.Domain.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;

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
            Console.WriteLine(actual.Item1 + "  " + actual.Item2);
        }

        [TestMethod]
        public void GetPngDimensionsTes2t()
        {
            string url = "https://www.dropbox.com/s/29r2qyxe510om7i/ADA-BLANK.jpg?raw=1";
            var bytesArr = DownloadHelper.GetImageFromUrl(url, new Dictionary<string, string>());
            MemoryStream memstr = new MemoryStream(bytesArr);
            Image img = Image.FromStream(memstr);
            Console.WriteLine(img.Width + "  " + img.Height);
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
            string url = "https://gdb.voanews.com/0FAABDC3-D51B-4F7E-B051-1CC4C81CAB21_cx0_cy6_cw0_w1023_r1_s.jpg";
            Uri uri = new Uri(url);
            var actual = ImageUtilities.GetWebDimensions(uri);
            Assert.AreEqual(new Size(250, 297), actual);
        }

        [TestMethod]
        public void IsActionExistsTest()
        {
            //var ttt = IsActionExists("Index", "Products");
            //Console.WriteLine(ttt);
            //ttt = IsActionExists("Detail", "Products");
            //Console.WriteLine(ttt);

            //Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            //foreach (var item in assemblies)
            //{
            //    Console.WriteLine(item.FullName);
            //}
            string path = @"C:\Users\Yuce\Desktop\Atomic E-mail";
            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
            var p = new List<String>();
            DataTable emails = new DataTable();
            emails.Columns.Add("Email");

            foreach (var f in files)
            {
                try
                {
                    var dt = ExcelHelper.ExcelToDataTable(f, ExcelHelper.GetWorkSheets(f).FirstOrDefault());
                    Console.WriteLine(dt.Rows.Count);
                    p.Add(f);

                    var dataRow = dt.AsEnumerable()
                  .Select(x => new
                  {
                      Email = x.Field<string>("E-mail address")
                  });
                    foreach (var dr in dataRow)
                    {
                        emails.Rows.Add(dr.Email);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            string connectionString = @"data source=devsqlserver;Integrated Security=SSPI;Initial Catalog=TestEY";
            emails.TableName = "_temp_Emails_Kathleen201805";
            ExcelHelper.SaveTable(emails, connectionString);
            //  Console.WriteLine("emails:" + emails.Rows.Count);
            foreach (var item in p)
            {
                Console.WriteLine(item);
                File.Delete(item);
            }
        }

        public bool IsActionExists(String action, String controller)
        {
            //var controllerFullName = string.Format("EImece.Areas.Amp.Controllers.{0}Controller", controller);
            //var assembly = Assembly.GetAssembly(typeof("EImece"));
            //var cont = assembly.GetType(controllerFullName);

            //if (cont != null)
            //{
            //    if (cont.GetMethod(action) != null)
            //    {
            //        return true;
            //    }
            //}
            return false;
        }
    }
}