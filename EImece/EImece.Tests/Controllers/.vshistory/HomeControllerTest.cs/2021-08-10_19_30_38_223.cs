using EImece.App_Start;
using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.FrontModels.Il_Ilce_Mahalle;
using EImece.Domain.Repositories;
using EImece.Domain.Services;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Ninject;
using NLog;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace EImece.Tests.Controllers
{
    public class TestClass
    {
        public int CurrentTaskId { get; set; }
        public int Index { get; set; }
        public string JobId { get; set; }
    }

    [TestClass]
    [DeploymentItem("EntityFramework.SqlServer.dll")]
    public class HomeControllerTest
    {
        private IKernel kernel = null;
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void BindByReflection(Type typeOfInterface, string typeofText)
        {
            var baseServiceTypes = Assembly.GetAssembly(typeOfInterface)
                .GetTypes().Where(t => t.Name.Contains(typeofText)).ToList();

            foreach (var type in baseServiceTypes)
            {
                var interfaceType = type.GetInterface("I" + type.Name);
                if (interfaceType != null)
                {
                    Console.WriteLine("interfaceType:" + interfaceType.Name + "  Name: " + type.Name);
                }
            }
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
            kernel = NinjectWebCommon.CreateKernel();
            Console.WriteLine("Reflecting Repository Assemblies");
        }

        [TestMethod]
        public void TestEmail()
        {
            var mail = new MailMessage();
            var SmtpServer = new SmtpClient("srvm02.turhost.com");
            mail.From = new MailAddress("test@websiteniz.com");
            mail.To.Add("test@websiteniz.com");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            mail.Subject = "Test Mail";
            mail.Body = "This is for testing SMTP mail";
            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential("test@websiteniz.com", "x2V8KkMx");
            SmtpServer.EnableSsl = false;
            SmtpServer.Send(mail);
        }

        [TestMethod]
        public void GetEmailAccount()
        {
            using (var db = new EImeceContext(ConnectionString))
            {
                var repository = new SettingRepository(db);
                var SettingService = new SettingService(repository);
                SettingService.DataCachingProvider = new LazyCacheProvider();
                EmailAccount emailAccount = SettingService.GetEmailAccount();
                Console.WriteLine(emailAccount.ToString());
                EmailSender emailSender = new EmailSender();
                //    emailSender.SendEmail("eminyuce@gmail.com", "Test", "TESTING", emailAccount);
            }
        }

        [TestMethod]
        public void GetDeserializeObjectProductSpecItem()
        {
            var ooo = JsonConvert.DeserializeObject<ProductSpecItemRoot>("{\"selectedTotalSpecs\":[{\"SpecsName\":\"color\",\"SpecsValue\":\"Kirmizi\"}]}");
            var selectedTotalSpecs = ooo.selectedTotalSpecs;
        }

        private String ConnectionString { get { return Constants.DbConnectionKey; } }

        private int CurrentLanguage
        {
            get
            {
                return AppConfig.MainLanguage;
            }
        }

        public static String HtmlToFBHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var brtags = doc.DocumentNode.SelectNodes("//br");
            var divNodes = doc.DocumentNode.SelectSingleNode("//div");

            HtmlNode paragraghNode = doc.CreateElement("p");
            var DivTags = doc.DocumentNode.SelectNodes("div");
            for (int i = 0; i < DivTags.Count; i++)
            {
                if (i > 0)
                {
                    doc.DocumentNode.InsertBefore(paragraghNode, DivTags[i]);
                }
                doc.DocumentNode.InsertBefore(HtmlNode.CreateNode(DivTags[i].InnerHtml), DivTags[i]);
                DivTags[i].ParentNode.RemoveChild(DivTags[i]);
            }

            if (brtags.Count <= 5)
            {
                return html;
            }
            else
            {
                foreach (var br in brtags)
                {
                    br.OuterHtml.Replace("<br>", "\n");
                }
            }
            return doc.DocumentNode.OuterHtml;
        }

        [TestMethod]
        public void HtmlToFBHtmlTesting()
        {
            //String html = "";
            //var html2 = HtmlToFBHtml(html);

            //Console.WriteLine(html2);

            //Parallel.For(0, 1000, i =>
            //{
            //    Console.WriteLine("i = {0}, thread = {1}", i,
            //    Thread.CurrentThread.ManagedThreadId);
            //     Thread.Sleep(10);
            //});

            int size = 1000000;
            double[] data = new double[size];
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < size; i++)
            {
                data[i] = Math.Pow(new Random().NextDouble(), 0.6);
            }
            Console.WriteLine("done " + sw.Elapsed.TotalMilliseconds.ToString());
            sw.Reset();

            sw.Start();
            Parallel.For(0, size, i =>
            {
                data[i] = Math.Pow(new Random().NextDouble(), 0.6);
            });
            sw.Stop();
            Console.WriteLine("done " + sw.Elapsed.TotalMilliseconds.ToString());
        }

        [TestMethod]
        public void GetProductDetailViewModelById()
        {
            var db = new EImeceContext(ConnectionString);
            var ProductService = new ProductService(new ProductRepository(db));
            var product = ProductService.GetProductDetailViewModelById(175363);
            Assert.IsTrue(product.RelatedProducts.Count > 0);
        }
       
        [TestMethod]
        public void GetShoppingSession()
        {
            String sessionJson = File.ReadAllText(@"C:\Users\YUCE\Documents\GitHub\EImece\EImece\EImece.Tests\dataFolder\shoppingSession.txt");
            var shoppingCart = JsonConvert.DeserializeObject<ShoppingCartSession>(sessionJson);
            Console.WriteLine(shoppingCart.ShoppingCartItems.Count);
            Assert.IsNotNull(shoppingCart.ShoppingCartItems);
        }

        [TestMethod]
        public void GetBuyNow()
        {
            String sessionJson = File.ReadAllText(@"C:\Users\YUCE\Documents\GitHub\EImece\EImece\EImece.Tests\dataFolder\BuyNowModel.txt");
            var buyNowModel = JsonConvert.DeserializeObject<BuyNowModel>(sessionJson);
            Assert.IsNotNull(buyNowModel.ShoppingCartItem);
        }

        [TestMethod]
        public void GetEmailTemplateById()
        {
            var db = new EImeceContext(ConnectionString);
            var service = new MailTemplateService(new MailTemplateRepository(db));
            service.DataCachingProvider = new MemoryCacheProvider();
            var orderConfirmationEmailTemplate = service.GetMailTemplateByName("OrderConfirmationEmail");
            Assert.IsNotNull(orderConfirmationEmailTemplate);
            String orderConfirmationEmailTemplateHtml = File.ReadAllText(@"C:\Users\YUCE\Documents\GitHub\EImece\EImece\EImece.Tests\dataFolder\emailTemplates\OrderConfirmationEmail.html");
            var aservice = new AddressService(new AddressRepository(db));
            var cservice = new CustomerService(new CustomerRepository(db), aservice);
            var opservice = new OrderProductService(new OrderProductRepository(db));
            var oservice = new OrderService(new OrderRepository(db), cservice, opservice);
            var cOrder = oservice.GetOrderById(12);
            Customer customer = cservice.GetUserId("44a72377-7a04-49ec-b8bb-40b9140deddc");
            var pp = new OrderConfirmationEmailRazorTemplate();
            pp.CompanyAddress = "3828 Mall Road  Los Angeles, California, 90017";
            pp.CompanyEmailAddress = "info@gmail.com";
            pp.CompanyPhoneNumber = "05456687854";
            pp.FinishedOrder = cOrder;
            pp.OrderProducts = cOrder.OrderProducts.ToList();
            string result = Engine.Razor.RunCompile(orderConfirmationEmailTemplateHtml, "Test", null, pp);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetBreadCrumb()
        {
            var db = new EImeceContext(ConnectionString);
            var ProductCategoryService = new ProductCategoryService(new ProductCategoryRepository(db));
            ProductCategoryService.DataCachingProvider = new LazyCacheProvider();
            var breadCrumb = ProductCategoryService.GetBreadCrumb(215, 1);
            foreach (var item in breadCrumb)
            {
                Console.WriteLine(item.ProductCategory.Id);
            }
        }

        [TestMethod]
        public void GetmahalleStr()
        {
            //System.IO.File.ReadAllText(Server.MapPath(@"~/App_Data/file.txt"));
            var ilceStr = File.ReadAllText(@"C:\Users\YUCE\Documents\GitHub\EImece\EImece\EImece\App_Data\il-ilce-mahalle\ilceler.json");
            var illerStr = File.ReadAllText(@"C:\Users\YUCE\Documents\GitHub\EImece\EImece\EImece\App_Data\il-ilce-mahalle\iller.json");
            var IlceRoot = JsonConvert.DeserializeObject(ilceStr, typeof(IlceRoot));
            var IlRoot = JsonConvert.DeserializeObject(illerStr, typeof(IlRoot));
        }

        [TestMethod]
        public void GenerateSql()
        {
            var sql = File.ReadAllText(@"C:\Users\YUCE\Documents\GitHub\EImece\EImece\EImece.Tests\dataFolder\sql.txt");
            var lines = File.ReadAllLines(@"C:\Users\YUCE\Documents\GitHub\EImece\EImece\EImece.Tests\dataFolder\productNames.txt");
            foreach (var line in lines)
            {
                Console.WriteLine(sql.Replace("[REPLACE_TEXT]", line).Replace("[ProductCodeValue]", GeneralHelper.RandomNumber(6) + ""));
            }
        }

        [TestMethod]
        public void ReadAllBytesImages()
        {
            var imageBytes = File.ReadAllBytes(@"‪C:\Users\YUCE\Desktop\vesikalik.jpg");
        }

        [TestMethod]
        public void ReadSiteMapXmlAndRequest()
        {
            var oooo = File.ReadAllText(@"C:\Users\YUCE\Documents\GitHub\EImece\EImece\EImece.Tests\dataFolder\siteMap2.xml");
            ReadSiteMapXmlAndRequest(oooo);
        }

        public void ReadSiteMapXmlAndRequest(string xml)
        {
            if (String.IsNullOrEmpty(xml))
            {
                return;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Urlset));
            using (StringReader reader = new StringReader(xml))
            {
                var test = (Urlset)serializer.Deserialize(reader);
                foreach (var tUrl in test.Url)
                {
                    var buffer = GeneralHelper.GetImageFromUrl(tUrl.Loc);
                }
            }
        }

        [TestMethod]
        public void GetActiveBaseContents()
        {
            var xdoc = XDocument.Parse(File.ReadAllText(@"C:\Users\YUCE\Documents\GitHub\EImece\EImece\EImece.Tests\dataFolder\ProductTemplate.xml"));
            var groups = xdoc.Root.Descendants("group");

            foreach (var group in groups)
            {
                foreach (XElement field in group.Elements())
                {
                    var name = field.Attribute("name");
                    var unit = field.Attribute("unit");
                    var values = field.Attribute("values");
                    var display = field.Attribute("display");
                    // var dbValueObj = specs.FirstOrDefault(r => r.Name.Equals(name.Value, StringComparison.InvariantCultureIgnoreCase));
                    //  if (dbValueObj == null)
                    //  {
                    //      dbValueObj = new ProductSpecification();
                    //  }
                    var dbValueObj = new ProductSpecification();
                    dbValueObj.FieldFormat = field;

                    Console.WriteLine("1)" + name + "1.1)" + name.Value + " 2)" + unit + " 3)" + values + " 4)" + display + "5)" + field.Name.LocalName);
                }
            }
        }

        [TestMethod]
        public void Index()
        {
            String search = "";
            var db = new EImeceContext(ConnectionString);
            var ProductCategoryService = new ProductCategoryService(new ProductCategoryRepository(db));
            Expression<Func<ProductCategory, bool>> whereLambda = r => r.Name.Contains(search);
            var productCategories = ProductCategoryService.SearchEntities(whereLambda, search, CurrentLanguage);
            foreach (var item in productCategories)
            {
                Console.WriteLine(item.Id);
                Console.WriteLine(item.MainImageId);
                Console.WriteLine(item.MainImage.FileName);
            }
        }

        [TestMethod]
        public void RegexTest()
        {
            String validationDetail = @"

MX:sitemail2.everyone.net.

HELO maritimereporter.com

250 m0087083.mta.everyone.net

MAIL FROM:<update@maritimereporter.com>

250 Sender okay

RCPT TO:<gregw@traversebaymarine.com>

999 Recipient okay

QUITE

";
            int startIndex = validationDetail.IndexOf("RCPT TO:");

            Regex regex = new Regex(@"^\d{3}", RegexOptions.Multiline);
            var d = regex.Matches(validationDetail.Substring(startIndex)).Cast<Match>()
               .Select(match => match.Value)
               .ToArray();

            Console.WriteLine(d.Last());
        }

        [TestMethod]
        public void About()
        {
            try
            {
                var db = new EImeceContext(ConnectionString);
                foreach (var item in db.ProductCategories)
                {
                    Console.WriteLine(item.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace.ToString());
            }
        }

        [TestMethod]
        public void Contact()
        {
            SqlConnection cnn;
            string connetionString2 = ConfigurationManager.ConnectionStrings[ConnectionString].ConnectionString;
            cnn = new SqlConnection(connetionString2);
            try
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = cnn;
                cmd.CommandText = "select getdate() date";
                cmd.ExecuteNonQuery();
                cnn.Close();
                Console.WriteLine("It is done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace.ToString());
            }
        }

        [TestMethod]
        public void DateTimeOffsetParse()
        {
            Console.WriteLine(DateTimeOffset.ParseExact("2014-12-11T04:44:16Z", "yyyy-MM-dd'T'HH:mm:ss'Z'",
                                                       CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void TestParsing()
        {
            String _contacts = File.ReadAllText(@"C:\Users\Yuce\Downloads\testFile.txt");
            var lines = Regex.Split(_contacts, @"\n").Select(r => r.Trim()).Where(s => !String.IsNullOrEmpty(s)).ToList();

            foreach (var line in lines)
            {
                int firstComma = line.IndexOf(",");
                var email = firstComma > 0 ? line.Substring(0, firstComma).Trim() : line.Trim();

                int secondComma = firstComma > 0 ? line.IndexOf(",", firstComma + 1) : -1;

                var name = "";
                if (firstComma > 0)
                {
                    name = secondComma > 0 ?
                        line.Substring(firstComma + 1, secondComma - firstComma).Trim() :
                        line.Substring(firstComma + 1).Trim();
                }

                var description = "";
                if (secondComma > 0)
                {
                    description = line.Substring(secondComma + 1).Trim();
                }

                Console.WriteLine("Email =" + email + " name= " + name + "  description=  " + description);
                //_destOtherContacts.Add(new NwmDestOtherContact() { Email = email, Name = name, Description = description });
            }
        }

        [TestMethod]
        public void DeleteProductCategory()
        {
            var dbContext = new EImeceContext(ConnectionString);
            FileStorageService fileStorageService = new FileStorageService(new FileStorageRepository(dbContext)); ;
            ProductService productService = new ProductService(new ProductRepository(dbContext));
            ProductCategoryService productCategoryService = new ProductCategoryService(new ProductCategoryRepository(dbContext));
            productCategoryService.ProductService = productService;
            productCategoryService.FileStorageService = fileStorageService;
            productCategoryService.DeleteProductCategory(308);
        }

        [TestMethod]
        public void TestAllRepository()
        {
            var dbContext = new EImeceContext(ConnectionString);

            Console.WriteLine(new FileStorageRepository(dbContext).GetAll().ToList().Count());
            Console.WriteLine(new MenuRepository(dbContext).GetAll().ToList().Count());
            Console.WriteLine(new ProductCategoryRepository(dbContext).GetAll().ToList().Count());
            Console.WriteLine(new ProductRepository(dbContext).GetAll().ToList().Count());
            Console.WriteLine(new ProductSpecificationRepository(dbContext).GetAll().ToList().Count());
            Console.WriteLine(new ProductTagRepository(dbContext).GetAll().ToList().Count());
            Console.WriteLine(new StoryCategoryRepository(dbContext).GetAll().ToList().Count());
            Console.WriteLine(new StoryFileRepository(dbContext).GetAll().ToList().Count());
            Console.WriteLine(new StoryRepository(dbContext).GetAll().ToList().Count());
            Console.WriteLine(new StoryTagRepository(dbContext).GetAll().ToList().Count());
            Console.WriteLine(new SubscriberRepository(dbContext).GetAll().ToList().Count());
            Console.WriteLine(new TagCategoryRepository(dbContext).GetAll().ToList().Count());
            Console.WriteLine(new TagRepository(dbContext).GetAll().ToList().Count());
        }

        [TestMethod]
        public void TestTotalDayOutsideOfUSA()
        {
            int totalDayOutsideOfUSA = 1; //Bahama trip 9 mayis 2014
            DateTime startTime = "04/02/2013".ToDateTime();
            DateTime endTime = "04/29/2013".ToDateTime();

            TimeSpan span = endTime.Subtract(startTime);
            Console.WriteLine(span.Days);
            totalDayOutsideOfUSA += span.Days;

            startTime = "09/11/2014".ToDateTime();
            endTime = "09/28/2014".ToDateTime();

            span = endTime.Subtract(startTime);
            Console.WriteLine(span.Days);
            totalDayOutsideOfUSA += span.Days;

            startTime = "11/11/2015".ToDateTime();
            endTime = "11/27/2015".ToDateTime();

            span = endTime.Subtract(startTime);
            Console.WriteLine(span.Days);
            totalDayOutsideOfUSA += span.Days;

            startTime = "11/14/2016".ToDateTime();
            endTime = "11/27/2016".ToDateTime();

            span = endTime.Subtract(startTime);
            Console.WriteLine(span.Days);
            totalDayOutsideOfUSA += span.Days;

            DateTime firstUsaEntrance = "07/03/2012".ToDateTime();
            DateTime fiveYearsLaterUsaEntrance = "07/03/2017".ToDateTime();
            DateTime fiveYearsLaterUsaEntrance2 = "04/03/2017".ToDateTime();

            DateTime todayDate = DateTime.Now;
            TimeSpan t = todayDate.Subtract(firstUsaEntrance);

            Console.WriteLine("Total Days of outside of USA:" + totalDayOutsideOfUSA);
            Console.WriteLine("Total Days I spent in USA:" + (t.Days - totalDayOutsideOfUSA) + "  " + fiveYearsLaterUsaEntrance2.Subtract(firstUsaEntrance).Days);
            Console.WriteLine("Application Date:" + fiveYearsLaterUsaEntrance2.AddDays(totalDayOutsideOfUSA).ToShortDateString());
        }

        [TestMethod]
        public void MathLog()
        {
            String[] numbers = {"	1	",
"	2	",
"	5	",
"	10	",
"	100	",
"	200	",
"	400	",
"	800	",
"	1600	",
"	3200	",
"	6400	",
"	12800	",
"	25600	",
"	51200	",
"	102400	",
"	204800	",
"	409600	",
"	819200	",
"	1638400	",
"	3276800	",
"	6553600	",
"	13107200	",
"	26214400	",
"	52428800	",
"	104857600	",
"	209715200	",
};

            foreach (var i in numbers)
            {
                Console.WriteLine(i.Trim().ToInt() + "   " + Math.Floor(Math.Log(i.Trim().ToInt() + 7)));
            }
        }

        [TestMethod]
        [Timeout(TestTimeout.Infinite)]
        public void SaveExcelFile()
        {
            var filePath = @"C:\Users\Yuce\Desktop\products.xls";
            var workSheetName = ExcelHelper.GetWorkSheets(filePath).FirstOrDefault();
            var dt = ExcelHelper.ExcelToDataTable(filePath, workSheetName);
            dt.TableName = "dbo.thelud_products";
            ExcelHelper.SaveTable(dt, @"data source=devsqlserver;Integrated Security=SSPI;Initial Catalog=TestEY");
        }
    }
}