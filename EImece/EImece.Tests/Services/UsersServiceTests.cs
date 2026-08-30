using EImece.Domain;
using EImece.Domain.Services;
using EImece.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Tests.Services
{
    [TestClass]
    public class UsersServiceTests
    {
        [TestMethod]
        public void SelectUserRolesViewModel_PopulateAdminRoles_FiltersToAdminAndEditorRoles()
        {
            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "admin@example.com",
                FirstName = "Admin",
                LastName = "User"
            };

            var adminRole = new IdentityRole { Id = "role-admin", Name = Constants.AdministratorRole };
            var editorRole = new IdentityRole { Id = "role-editor", Name = Constants.EditorRole };
            var customerRole = new IdentityRole { Id = "role-customer", Name = Constants.CustomerRole };

            user.Roles.Add(new IdentityUserRole { RoleId = "role-admin", UserId = "user-1" });

            var model = new SelectUserRolesViewModel(user);
            model.PopulateAdminRoles(user, new List<IdentityRole> { adminRole, editorRole, customerRole });

            Assert.AreEqual(2, model.Roles.Count);
            Assert.IsTrue(model.Roles.Any(r => r.RoleName == Constants.AdministratorRole && r.Selected));
            Assert.IsTrue(model.Roles.Any(r => r.RoleName == Constants.EditorRole && !r.Selected));
            Assert.IsFalse(model.Roles.Any(r => r.RoleName == Constants.CustomerRole));
        }

        [TestMethod]
        public void SelectUserRolesViewModel_PopulateRoles_IncludesAllRoles()
        {
            var user = new ApplicationUser
            {
                Id = "user-2",
                UserName = "customer@example.com",
                FirstName = "Jane",
                LastName = "Doe"
            };

            var adminRole = new IdentityRole { Id = "role-admin", Name = Constants.AdministratorRole };
            var customerRole = new IdentityRole { Id = "role-customer", Name = Constants.CustomerRole };

            user.Roles.Add(new IdentityUserRole { RoleId = "role-customer", UserId = "user-2" });

            var model = new SelectUserRolesViewModel(user);
            model.PopulateRoles(user, new List<IdentityRole> { adminRole, customerRole });

            Assert.AreEqual(2, model.Roles.Count);
            Assert.IsTrue(model.Roles.Any(r => r.RoleName == Constants.CustomerRole && r.Selected));
            Assert.IsTrue(model.Roles.Any(r => r.RoleName == Constants.AdministratorRole && !r.Selected));
        }

        [TestMethod]
        public void UsersService_Constructor_ThrowsOnNullDbContext()
        {
            try
            {
                var service = new UsersService(null, null, null, null);
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void TurkishRegionService_GetTownsByCity_ReturnsEmptyForUnknownCity()
        {
            var service = new TurkishRegionService();
            var towns = service.GetTownsByCity("NonExistentCity");
            Assert.IsNotNull(towns);
            Assert.AreEqual(0, towns.Count);
        }

        [TestMethod]
        public void RazorEngineHelper_Constructor_ThrowsOnNullDependency()
        {
            try
            {
                var helper = new EImece.Domain.Helpers.EmailHelper.RazorEngineHelper(null, null, null, null, null, null);
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }
    }
}
