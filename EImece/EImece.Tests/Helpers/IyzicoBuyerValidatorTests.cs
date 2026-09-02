using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class IyzicoBuyerValidatorTests
    {
        [TestMethod]
        public void ReservedTestTld_IsRejected()
        {
            Assert.IsFalse(IyzicoBuyerValidator.IsIyzicoAcceptedEmail("seeduser00010@eimece.test"));
            var errors = IyzicoBuyerValidator.Validate(ValidCustomer("seeduser00010@eimece.test"));
            Assert.IsTrue(errors.Any(e => e.Field == "Email"));
        }

        [TestMethod]
        public void OtherReservedTlds_AreRejected()
        {
            Assert.IsFalse(IyzicoBuyerValidator.IsIyzicoAcceptedEmail("user@domain.invalid"));
            Assert.IsFalse(IyzicoBuyerValidator.IsIyzicoAcceptedEmail("user@domain.example"));
            Assert.IsFalse(IyzicoBuyerValidator.IsIyzicoAcceptedEmail("user@foo.localhost"));
        }

        [TestMethod]
        public void PublicLookingEmail_IsAccepted()
        {
            Assert.IsTrue(IyzicoBuyerValidator.IsIyzicoAcceptedEmail("buyer@gmail.com"));
            Assert.IsTrue(IyzicoBuyerValidator.IsValid(ValidCustomer("buyer@gmail.com")));
        }

        [TestMethod]
        public void EmailIsNotRewritten()
        {
            var customer = ValidCustomer("seeduser00010@eimece.test");
            IyzicoBuyerValidator.Validate(customer);
            Assert.AreEqual("seeduser00010@eimece.test", customer.Email);
        }

        [TestMethod]
        public void MissingRequiredBuyerFields_ReturnErrors()
        {
            var errors = IyzicoBuyerValidator.Validate(new CustomerDto());
            CollectionAssert.Contains(errors.Select(e => e.Field).ToList(), "Name");
            CollectionAssert.Contains(errors.Select(e => e.Field).ToList(), "Surname");
            CollectionAssert.Contains(errors.Select(e => e.Field).ToList(), "Email");
            CollectionAssert.Contains(errors.Select(e => e.Field).ToList(), "GsmNumber");
            CollectionAssert.Contains(errors.Select(e => e.Field).ToList(), "IdentityNumber");
            CollectionAssert.Contains(errors.Select(e => e.Field).ToList(), "City");
            CollectionAssert.Contains(errors.Select(e => e.Field).ToList(), "Country");
        }

        [TestMethod]
        public void IdentityNumber_MustBeElevenDigits()
        {
            var customer = ValidCustomer("buyer@gmail.com");
            customer.IdentityNumber = "1234567890a";
            var errors = IyzicoBuyerValidator.Validate(customer);
            Assert.IsTrue(errors.Any(e => e.Field == "IdentityNumber"));
        }

        [TestMethod]
        public void InvalidGsm_IsRejected()
        {
            var customer = ValidCustomer("buyer@gmail.com");
            customer.GsmNumber = "123";
            var errors = IyzicoBuyerValidator.Validate(customer);
            Assert.IsTrue(errors.Any(e => e.Field == "GsmNumber"));
        }

        private static CustomerDto ValidCustomer(string email)
        {
            return new CustomerDto
            {
                Name = "Ali",
                Surname = "Yilmaz",
                Email = email,
                GsmNumber = "5321234567",
                IdentityNumber = "12345678901",
                City = "Istanbul",
                Country = "Turkey",
                Street = "Ornek Sokak 1"
            };
        }
    }
}
