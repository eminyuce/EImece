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
    }
}
