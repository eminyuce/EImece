using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EImece;
using EImece.Controllers;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Repositories;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using System.Linq.Expressions;
using EImece.Domain.Services;
using System.Data.SqlClient;
using System.Configuration;
using EImece.Domain;
using EImece.Domain.Helpers;
using System.Text.RegularExpressions;

namespace EImece.Tests.Controllers
{
    [TestClass]
    [DeploymentItem("EntityFramework.SqlServer.dll")]
    public class HomeControllerTest
    {

        private String ConnectionString { get { return Settings.DbConnectionKey; } }
        private int CurrentLanguage
        {
            get
            {
                return Settings.MainLanguage;
            }
        }
        [TestMethod]
        public void Index()
        {
            String search = "";
            var db = new EImeceContext(ConnectionString);
            var ProductCategoryService = new ProductCategoryService(new ProductCategoryRepository(db));
            Expression<Func<ProductCategory, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
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
            String validationDetail = @"MX:sitemail2.everyone.net.
HELO maritimereporter.com

250 m0087083.mta.everyone.net
MAIL FROM:<update@maritimereporter.com>

250 Sender okay
RCPT TO:<gregw@traversebaymarine.com>

999 Recipient okay
QUITE
";
            int startIndex = validationDetail.IndexOf("RCPT TO:");

            Regex regex = new Regex(@"^\d{3}",RegexOptions.Multiline);
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
                String search = "";
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
        public void GetTagsByTagType()
        {
            var dbContext = new EImeceContext(ConnectionString);
            var tr = new TagCategoryRepository(dbContext);
            var tags = tr.GetTagsByTagType(EImeceLanguage.Turkish);
            Console.WriteLine(tags.Count);
        }

        [TestMethod]
        public void GetmenuLink()
        {
            String menuLink = "stories-category_eray-notlar-52";
            String m =menuLink.Split("_".ToCharArray()).Last();

            Console.WriteLine(m);
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
    }
}
