using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using EImece.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests.Unit.Controllers
{
    [TestClass]
    public class AccountControllerUnitTests
    {
        [TestMethod]
        public void LoginViewModel_RequiresEmailAndPassword()
        {
            var model = new LoginViewModel();
            var results = Validate(model);
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains(nameof(LoginViewModel.Email))));
            Assert.IsTrue(results.Any(r => r.MemberNames.Contains(nameof(LoginViewModel.Password))));
        }

        [TestMethod]
        public void LoginViewModel_ValidWhenEmailAndPasswordSet()
        {
            var model = new LoginViewModel
            {
                Email = "user@example.com",
                Password = "Secret1!"
            };
            var results = Validate(model);
            Assert.AreEqual(0, results.Count);
        }

        private static List<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }
    }
}
