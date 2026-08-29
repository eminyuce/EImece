using EImece.Domain.Helpers;
using EImece.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class UserAdminFilterHelperTests
    {
        [TestMethod]
        public void Apply_FiltersByRoleIgnoringCase()
        {
            var result = UserAdminFilterHelper.Apply(SampleUsers(), "administrator", null, null).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("admin@eimece.test", result[0].Email);
        }

        [TestMethod]
        public void Apply_FiltersTwoFactorOnAndOff()
        {
            var on = UserAdminFilterHelper.Apply(SampleUsers(), null, "yes", null).ToList();
            var off = UserAdminFilterHelper.Apply(SampleUsers(), null, "no", null).ToList();

            Assert.AreEqual(1, on.Count);
            Assert.AreEqual("admin@eimece.test", on[0].Email);
            Assert.AreEqual(2, off.Count);
        }

        [TestMethod]
        public void Apply_FiltersLockedAccounts()
        {
            var locked = UserAdminFilterHelper.Apply(SampleUsers(), null, null, "1").ToList();
            var unlocked = UserAdminFilterHelper.Apply(SampleUsers(), null, null, "no").ToList();

            Assert.AreEqual(1, locked.Count);
            Assert.AreEqual("locked@eimece.test", locked[0].Email);
            Assert.AreEqual(2, unlocked.Count);
        }

        [TestMethod]
        public void Apply_CombinesRoleAndLock()
        {
            var result = UserAdminFilterHelper.Apply(SampleUsers(), "Editor", null, "yes").ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("locked@eimece.test", result[0].Email);
        }

        [TestMethod]
        public void Apply_NullUsers_ReturnsEmpty()
        {
            Assert.AreEqual(0, UserAdminFilterHelper.Apply(null, "Admin", "yes", "yes").Count());
        }

        private static List<EditUserViewModel> SampleUsers()
        {
            return new List<EditUserViewModel>
            {
                new EditUserViewModel { Email = "admin@eimece.test", Role = "Administrator", AuthenticatorEnabled = true, IsLockedOut = false },
                new EditUserViewModel { Email = "editor@eimece.test", Role = "Editor", AuthenticatorEnabled = false, IsLockedOut = false },
                new EditUserViewModel { Email = "locked@eimece.test", Role = "Editor", AuthenticatorEnabled = false, IsLockedOut = true }
            };
        }
    }
}
