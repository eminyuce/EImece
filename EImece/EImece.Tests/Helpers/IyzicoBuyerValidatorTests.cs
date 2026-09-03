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
        public void Validate_NullCustomer_ReturnsTopLevelMandatoryError()
        {
            var errors = IyzicoBuyerValidator.Validate(null);
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(string.Empty, errors[0].Field);
            Assert.IsFalse(string.IsNullOrWhiteSpace(errors[0].Message));
            Assert.IsFalse(IyzicoBuyerValidator.IsValid(null));
        }

        [TestMethod]
        public void Validate_EmptyCustomer_ReturnsAllRequiredFieldErrors()
        {
            var customer = new CustomerDto { RegistrationAddress = "   " };
            var errors = IyzicoBuyerValidator.Validate(customer);
            var fields = errors.Select(e => e.Field).ToList();

            CollectionAssert.Contains(fields, "Name");
            CollectionAssert.Contains(fields, "Surname");
            CollectionAssert.Contains(fields, "Email");
            CollectionAssert.Contains(fields, "GsmNumber");
            CollectionAssert.Contains(fields, "IdentityNumber");
            CollectionAssert.Contains(fields, "City");
            CollectionAssert.Contains(fields, "Country");
            CollectionAssert.Contains(fields, "Street");
            Assert.IsFalse(IyzicoBuyerValidator.IsValid(customer));
        }

        [TestMethod]
        public void Validate_BuyerId_ResolutionBranches()
        {
            // Id > 0 -> valid buyerId
            var c1 = ValidCustomer("test@domain.com");
            c1.Id = 42;
            c1.UserId = null;
            var err1 = IyzicoBuyerValidator.Validate(c1);
            Assert.IsFalse(err1.Any(e => e.Field == "Id"));

            // Id == 0 with UserId -> valid buyerId
            var c2 = ValidCustomer("test@domain.com");
            c2.Id = 0;
            c2.UserId = "user-guid-12345";
            var err2 = IyzicoBuyerValidator.Validate(c2);
            Assert.IsFalse(err2.Any(e => e.Field == "Id"));

            // Id == 0 with whitespace UserId -> Id error
            var c3 = ValidCustomer("test@domain.com");
            c3.Id = 0;
            c3.UserId = "   ";
            // Note: Id is int, so customer.Id.ToString() is "0", which is not empty, so no error.
            var err3 = IyzicoBuyerValidator.Validate(c3);
            Assert.IsFalse(err3.Any(e => e.Field == "Id"));
        }

        [TestMethod]
        public void Validate_NameAndSurname_WhitespaceOrNull_ReturnsErrors()
        {
            var c1 = ValidCustomer("buyer@gmail.com");
            c1.Name = "  ";
            var err1 = IyzicoBuyerValidator.Validate(c1);
            Assert.IsTrue(err1.Any(e => e.Field == "Name"));

            var c2 = ValidCustomer("buyer@gmail.com");
            c2.Surname = null;
            var err2 = IyzicoBuyerValidator.Validate(c2);
            Assert.IsTrue(err2.Any(e => e.Field == "Surname"));
        }

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
            Assert.IsFalse(IyzicoBuyerValidator.IsIyzicoAcceptedEmail("user@domain.TEST")); // case insensitivity
        }

        [TestMethod]
        public void PublicLookingEmail_IsAccepted()
        {
            Assert.IsTrue(IyzicoBuyerValidator.IsIyzicoAcceptedEmail("buyer@gmail.com"));
            Assert.IsTrue(IyzicoBuyerValidator.IsIyzicoAcceptedEmail("customer.service@sub.my-domain.com.tr"));
            Assert.IsTrue(IyzicoBuyerValidator.IsValid(ValidCustomer("buyer@gmail.com")));
        }

        [TestMethod]
        public void EmailValidation_NullEmptyAndInvalidFormats_ReturnErrors()
        {
            Assert.IsFalse(IyzicoBuyerValidator.IsIyzicoAcceptedEmail(null));
            Assert.IsFalse(IyzicoBuyerValidator.IsIyzicoAcceptedEmail(""));
            Assert.IsFalse(IyzicoBuyerValidator.IsIyzicoAcceptedEmail("   "));
            Assert.IsFalse(IyzicoBuyerValidator.IsIyzicoAcceptedEmail("invalid-email"));
            Assert.IsFalse(IyzicoBuyerValidator.IsIyzicoAcceptedEmail("no-domain@"));
            Assert.IsFalse(IyzicoBuyerValidator.IsIyzicoAcceptedEmail("@no-user.com"));

            var c1 = ValidCustomer(null);
            var err1 = IyzicoBuyerValidator.Validate(c1);
            Assert.IsTrue(err1.Any(e => e.Field == "Email"));

            var c2 = ValidCustomer("not-an-email");
            var err2 = IyzicoBuyerValidator.Validate(c2);
            Assert.IsTrue(err2.Any(e => e.Field == "Email"));
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

            // 10 digits (too short)
            customer.IdentityNumber = "1234567890";
            Assert.IsTrue(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "IdentityNumber"));

            // 12 digits (too long)
            customer.IdentityNumber = "123456789012";
            Assert.IsTrue(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "IdentityNumber"));

            // null / empty
            customer.IdentityNumber = null;
            Assert.IsTrue(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "IdentityNumber"));

            // valid exactly 11 digits
            customer.IdentityNumber = "12345678901";
            Assert.IsFalse(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "IdentityNumber"));
        }

        [TestMethod]
        public void GsmNumber_ValidationRules()
        {
            var customer = ValidCustomer("buyer@gmail.com");

            // Too short / invalid
            customer.GsmNumber = "123";
            var errors = IyzicoBuyerValidator.Validate(customer);
            Assert.IsTrue(errors.Any(e => e.Field == "GsmNumber"));

            // Null or whitespace
            customer.GsmNumber = null;
            Assert.IsTrue(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "GsmNumber"));

            customer.GsmNumber = "   ";
            Assert.IsTrue(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "GsmNumber"));

            // Valid GSM formats
            customer.GsmNumber = "5321234567";
            Assert.IsFalse(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "GsmNumber"));

            customer.GsmNumber = "05321234567";
            Assert.IsFalse(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "GsmNumber"));

            customer.GsmNumber = "+905321234567";
            Assert.IsFalse(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "GsmNumber"));
        }

        [TestMethod]
        public void CityAndCountry_ValidationRules()
        {
            var customer = ValidCustomer("buyer@gmail.com");

            // Empty City
            customer.City = "  ";
            Assert.IsTrue(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "City"));
            customer.City = "Istanbul";
            Assert.IsFalse(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "City"));

            // Empty Country
            customer.Country = null;
            Assert.IsTrue(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "Country"));
            customer.Country = "Turkey";
            Assert.IsFalse(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "Country"));
        }

        [TestMethod]
        public void StreetAndRegistrationAddress_FallbackRules()
        {
            var customer = ValidCustomer("buyer@gmail.com");

            // Both Street and RegistrationAddress whitespace -> Error
            customer.Street = "   ";
            customer.RegistrationAddress = "   ";
            var errors = IyzicoBuyerValidator.Validate(customer);
            Assert.IsTrue(errors.Any(e => e.Field == "Street"));

            // Street present, RegistrationAddress empty -> Valid
            customer.Street = "Ataturk Cad. No: 10";
            customer.RegistrationAddress = "   ";
            Assert.IsFalse(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "Street"));

            // RegistrationAddress present, Street empty -> Valid
            customer.Street = null;
            customer.RegistrationAddress = "Bagdat Caddesi No: 5";
            Assert.IsFalse(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "Street"));

            // Both present -> Valid
            customer.Street = "Ataturk Cad. No: 10";
            customer.RegistrationAddress = "Bagdat Caddesi No: 5";
            Assert.IsFalse(IyzicoBuyerValidator.Validate(customer).Any(e => e.Field == "Street"));
        }

        [TestMethod]
        public void IyzicoBuyerFieldError_ConstructorAndProperties()
        {
            var error = new IyzicoBuyerFieldError("Email", "Invalid email address");
            Assert.AreEqual("Email", error.Field);
            Assert.AreEqual("Invalid email address", error.Message);

            var nullError = new IyzicoBuyerFieldError(null, null);
            Assert.AreEqual(string.Empty, nullError.Field);
            Assert.AreEqual(string.Empty, nullError.Message);
        }

        [TestMethod]
        public void HasReservedTld_EdgeCases()
        {
            // Email without @
            Assert.IsFalse(IyzicoBuyerValidator.IsIyzicoAcceptedEmail("plainstring"));
            // Email where @ is at the very end
            Assert.IsFalse(IyzicoBuyerValidator.IsIyzicoAcceptedEmail("user@"));
            // Email with domain without dot (e.g. user@localhost)
            Assert.IsFalse(IyzicoBuyerValidator.IsIyzicoAcceptedEmail("user@localhost"));
        }

        private static CustomerDto ValidCustomer(string email)
        {
            return new CustomerDto
            {
                Id = 1,
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
