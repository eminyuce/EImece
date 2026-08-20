using EImece.Areas.Admin.Controllers;
using EImece.Areas.Admin.Models;
using EImece.Controllers;
using EImece.Domain;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.FrontModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Tests.Controllers
{
    [TestClass]
    public class RssFeedsControllerTests
    {
        [TestMethod]
        public void RssFeedsController_ShouldInheritFromBaseAdminController()
        {
            var controller = new RssFeedsController();
            Assert.IsInstanceOfType(controller, typeof(BaseAdminController));
        }

        [TestMethod]
        public void BaseAdminController_ShouldHaveAuthorizeRolesAttribute()
        {
            var authAttribute = typeof(BaseAdminController).GetCustomAttribute<AuthorizeRolesAttribute>();
            Assert.IsNotNull(authAttribute, "BaseAdminController must have AuthorizeRolesAttribute");
            Assert.IsTrue(authAttribute.Roles.Contains(Constants.AdministratorRole));
            Assert.IsTrue(authAttribute.Roles.Contains(Constants.EditorRole));
        }

        [TestMethod]
        public async Task RssFeedsController_Index_ShouldReturnViewWithFeedsAndMetadata()
        {
            var controller = new RssFeedsController();

            var result = await controller.Index(CancellationToken.None) as ViewResult;

            Assert.IsNotNull(result, "Index action should return a ViewResult.");
            var model = result.Model as RssFeedsIndexViewModel;
            Assert.IsNotNull(model, "Model should be of type RssFeedsIndexViewModel.");
            Assert.AreEqual(4, model.Feeds.Count, "Should define 4 RSS feeds (products, productcategories, storycategories, storycategoriesfull).");

            // Verify Products RSS feed definition
            var productFeed = model.Feeds.FirstOrDefault(f => f.Key == "products");
            Assert.IsNotNull(productFeed, "Products feed must be defined.");
            Assert.AreEqual("/rss/products", productFeed.RelativePath);
            Assert.AreEqual("GET", productFeed.HttpMethod);
            Assert.AreEqual("application/rss+xml", productFeed.ContentType);
            Assert.IsFalse(productFeed.RequiresCategoryId);
            Assert.IsTrue(productFeed.Parameters.Any(p => p.Name == "Take"), "Products feed must support 'Take' parameter.");
            Assert.IsTrue(productFeed.Parameters.Any(p => p.Name == "Language"), "Products feed must support 'Language' parameter.");
            Assert.IsTrue(productFeed.Parameters.Any(p => p.Name == "Description"), "Products feed must support 'Description' parameter.");
            Assert.IsTrue(productFeed.Parameters.Any(p => p.Name == "Width"), "Products feed must support 'Width' parameter.");
            Assert.IsTrue(productFeed.Parameters.Any(p => p.Name == "Height"), "Products feed must support 'Height' parameter.");
            Assert.IsTrue(productFeed.Parameters.Any(p => p.Name == "utm_source"), "Products feed must support 'utm_source' tracking parameter.");

            // Verify Product Categories feed definition
            var productCategoriesFeed = model.Feeds.FirstOrDefault(f => f.Key == "productcategories");
            Assert.IsNotNull(productCategoriesFeed, "ProductCategories feed must be defined.");
            Assert.AreEqual("/rss/productcategories", productCategoriesFeed.RelativePath);
            Assert.IsTrue(productCategoriesFeed.RequiresCategoryId, "ProductCategories feed must require CategoryId.");
            var prodCatIdParam = productCategoriesFeed.Parameters.FirstOrDefault(p => p.Name == "CategoryId");
            Assert.IsNotNull(prodCatIdParam);
            Assert.IsTrue(prodCatIdParam.IsRequired);

            // Verify Story Categories summary feed definition
            var storyCategoriesFeed = model.Feeds.FirstOrDefault(f => f.Key == "storycategories");
            Assert.IsNotNull(storyCategoriesFeed, "StoryCategories feed must be defined.");
            Assert.AreEqual("/rss/storycategories", storyCategoriesFeed.RelativePath);
            Assert.IsTrue(storyCategoriesFeed.RequiresCategoryId, "StoryCategories feed must require CategoryId.");
            var categoryIdParam = storyCategoriesFeed.Parameters.FirstOrDefault(p => p.Name == "CategoryId");
            Assert.IsNotNull(categoryIdParam);
            Assert.IsTrue(categoryIdParam.IsRequired, "CategoryId must be marked as required in StoryCategories feed.");

            // Verify Story Categories full CDATA feed definition
            var storyCategoriesFullFeed = model.Feeds.FirstOrDefault(f => f.Key == "storycategoriesfull");
            Assert.IsNotNull(storyCategoriesFullFeed, "StoryCategoriesFull feed must be defined.");
            Assert.AreEqual("/rss/storycategoriesfull", storyCategoriesFullFeed.RelativePath);
            Assert.IsTrue(storyCategoriesFullFeed.RequiresCategoryId, "StoryCategoriesFull feed must require CategoryId.");
            Assert.IsTrue(storyCategoriesFullFeed.OutputFormat.Contains("CDATA"), "StoryCategoriesFull output format should specify CDATA HTML.");

            // Verify Languages list populated
            Assert.IsNotNull(model.Languages);
            Assert.IsTrue(model.Languages.Count > 0, "Languages list should be populated.");
        }

        [TestMethod]
        public void RssController_ProductCategories_ActionShouldExist()
        {
            var method = typeof(RssController).GetMethod("ProductCategories", new[] { typeof(RssParams) });
            Assert.IsNotNull(method, "RssController must have a ProductCategories(RssParams) action method.");
            Assert.IsTrue(typeof(Task<ActionResult>).IsAssignableFrom(method.ReturnType), "ProductCategories action must return Task<ActionResult>.");
        }

        [TestMethod]
        public async Task UnderConstructionController_Index_ShouldBeAsync()
        {
            var controller = new UnderConstructionController();
            var result = await controller.Index();
            Assert.IsNotNull(result, "UnderConstructionController Index should return an ActionResult.");
        }

        [TestMethod]
        public async Task RobotController_RobotsText_ShouldBeAsync()
        {
            var controller = new RobotController();
            var result = await controller.RobotsText();
            Assert.IsNotNull(result, "RobotController RobotsText should return a FileContentResult.");
            Assert.AreEqual(MediaTypeNames.Text.Plain, result.ContentType);
            Assert.IsTrue(result.FileContents.Length > 0);
        }
    }
}
