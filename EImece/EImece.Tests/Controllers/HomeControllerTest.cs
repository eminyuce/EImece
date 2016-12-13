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

namespace EImece.Tests.Controllers
{
    [TestClass]
    [DeploymentItem("EntityFramework.SqlServer.dll")]
    public class HomeControllerTest
    {

        private const String ConnectionString = "Stores";
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
        public void GetMainPageProducts()
        {
            IProductRepository rep = new ProductRepository(new EImeceContext(ConnectionString));
            var products = rep.GetMainPageProducts(100,0);


        }
    }
}
