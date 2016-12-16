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

namespace EImece.Tests.Controllers
{
    [TestClass]
    [DeploymentItem("EntityFramework.SqlServer.dll")]
    public class HomeControllerTest
    {

        private const String ConnectionString = "EImeceDbConnection";
        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void About()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.About() as ViewResult;

            // Assert
            Assert.AreEqual("Your application description page.", result.ViewBag.Message);
        }

        [TestMethod]
        public void Contact()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Contact() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void PagingTest()
        {
            var dbContext = new EImeceContext(ConnectionString);
            var p = new ProductRepository(dbContext);
            var products =  p.GetMainPageProducts(1);
        }
        [TestMethod]
        public void TestAllRepository()
        {
            var dbContext = new EImeceContext(ConnectionString);

            Console.WriteLine(new FileStorageRepository(dbContext).GetAll().ToList().Count());
            Console.WriteLine(new FileStorageTagRepository(dbContext).GetAll().ToList().Count());
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
    }
}
