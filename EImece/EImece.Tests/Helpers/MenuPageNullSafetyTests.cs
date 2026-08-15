using EImece.Domain.Models.FrontModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests.Helpers
{
    /// <summary>
    /// Documents the contract used by PagesController / HomeController after BUG-001:
    /// MenuService.GetPageById(Async) returns null when the Menus row is missing,
    /// and callers must treat that as NotFound instead of dereferencing Menu.
    /// </summary>
    [TestClass]
    public class MenuPageNullSafetyTests
    {
        [TestMethod]
        public void MenuPageViewModel_AllowsNullMenuWithoutConstructionFailure()
        {
            var model = new MenuPageViewModel
            {
                Menu = null,
                ApplicationSettings = null
            };

            Assert.IsNull(model.Menu);
            Assert.IsNull(model.ApplicationSettings);
        }

        [TestMethod]
        public void MenuPageViewModel_InitializesDefaultCollections()
        {
            var model = new MenuPageViewModel();

            Assert.IsNotNull(model.SideMenus);
            Assert.IsNotNull(model.ApplicationSettings);
            Assert.IsNotNull(model.SocialMediaLinks);
            Assert.IsNotNull(model.CompanyName);
            Assert.IsNotNull(model.GoogleMapScript);
        }

        [TestMethod]
        public void StorefrontMenuDto_InitializesDefaultCollectionsAndMenuFiles()
        {
            var dto = new EImece.Domain.Models.DTOs.Storefront.StorefrontMenuDto();

            Assert.IsNotNull(dto.Children);
            Assert.IsNotNull(dto.SideMenus);
            Assert.IsNotNull(dto.MenuFiles);
            Assert.AreEqual(0, dto.MenuFiles.Count);
        }

        [TestMethod]
        public void StorefrontMenuFileDto_ConstructsAndPropertiesWork()
        {
            var fileDto = new EImece.Domain.Models.DTOs.Storefront.StorefrontMenuFileDto
            {
                Id = 1,
                MenuId = 10,
                FileStorageId = 100,
                FileName = "test.jpg",
                Name = "Test File",
                Position = 2,
                IsActive = true
            };

            Assert.AreEqual(1, fileDto.Id);
            Assert.AreEqual(10, fileDto.MenuId);
            Assert.AreEqual(100, fileDto.FileStorageId);
            Assert.AreEqual("test.jpg", fileDto.FileName);
            Assert.AreEqual(2, fileDto.Position);
            Assert.IsTrue(fileDto.IsActive);
        }

        [TestMethod]
        public void StorefrontPageDto_IncludesPageTheme()
        {
            var pageDto = new EImece.Domain.Models.DTOs.Storefront.StorefrontPageDto
            {
                Id = 5,
                Name = "About Us",
                PageTheme = "T3",
                IsActive = true
            };

            Assert.AreEqual("T3", pageDto.PageTheme);
            Assert.AreEqual(5, pageDto.Id);
            Assert.AreEqual("About Us", pageDto.Name);
        }
    }
}
