using EImece.Domain.Models.DTOs;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Field-level checks for Iyzico Checkout Form buyer + address inputs.
    /// Does not rewrite values. Official CF buyer requires id, name, surname,
    /// identityNumber, email, gsmNumber, registrationAddress, city, country
    /// (https://docs.iyzico.com/en/payment-methods/checkoutform/cf-implementation/cf-initialize).
    /// Iyzico error 5: reserved TLDs such as .test are rejected as invalid email format.
    /// </summary>
    public static class IyzicoBuyerValidator
    {
        // RFC 2606 / RFC 6761 reserved suffixes. Iyzico rejects these (error 5).
        private static readonly HashSet<string> ReservedTlds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "test",
            "example",
            "invalid",
            "localhost"
        };

        public static IList<IyzicoBuyerFieldError> Validate(CustomerDto customer)
        {
            var errors = new List<IyzicoBuyerFieldError>();

            if (customer == null)
            {
                errors.Add(new IyzicoBuyerFieldError(string.Empty, Resource.PleaseFillOutMandatoryBelowFields));
                return errors;
            }

            // CustomerDto.Id is int. IyzicoService sends customer.Id.ToStr() (never empty for int).
            var buyerId = customer.Id > 0
                ? customer.Id.ToString()
                : (string.IsNullOrWhiteSpace(customer.UserId) ? customer.Id.ToString() : customer.UserId);
            if (string.IsNullOrWhiteSpace(buyerId))
            {
                errors.Add(new IyzicoBuyerFieldError("Id", Resource.MandatoryField));
            }

            if (string.IsNullOrWhiteSpace(customer.Name))
            {
                errors.Add(new IyzicoBuyerFieldError("Name", Resource.PleaseEnterYourName));
            }

            if (string.IsNullOrWhiteSpace(customer.Surname))
            {
                errors.Add(new IyzicoBuyerFieldError("Surname", Resource.PleaseEnterYourSurname));
            }

            ValidateEmail(customer.Email, errors);
            ValidateGsm(customer.GsmNumber, errors);
            ValidateIdentityNumber(customer.IdentityNumber, errors);

            if (string.IsNullOrWhiteSpace(customer.City))
            {
                errors.Add(new IyzicoBuyerFieldError("City", Resource.PleaseEnterYourCity));
            }

            if (string.IsNullOrWhiteSpace(customer.Country))
            {
                errors.Add(new IyzicoBuyerFieldError("Country", Resource.PleaseEnterYourCountry));
            }

            if (string.IsNullOrWhiteSpace(customer.Street)
                && string.IsNullOrWhiteSpace(customer.RegistrationAddress))
            {
                errors.Add(new IyzicoBuyerFieldError("Street", Resource.PleaseEnterYourStreet));
            }

            return errors;
        }

        public static bool IsValid(CustomerDto customer)
        {
            return Validate(customer).Count == 0;
        }

        public static bool IsIyzicoAcceptedEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var trimmed = email.Trim();
            return GeneralHelper.IsValidEmail(trimmed) && !HasReservedTld(trimmed);
        }

        private static void ValidateEmail(string email, IList<IyzicoBuyerFieldError> errors)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add(new IyzicoBuyerFieldError("Email", Resource.PleaseEnterYourEmail));
                return;
            }

            var trimmed = email.Trim();
            if (GeneralHelper.IsNotValidEmail(trimmed) || HasReservedTld(trimmed))
            {
                errors.Add(new IyzicoBuyerFieldError("Email", Resource.IyzicoEmailNotValidMessage));
            }
        }

        private static void ValidateGsm(string gsmNumber, IList<IyzicoBuyerFieldError> errors)
        {
            if (string.IsNullOrWhiteSpace(gsmNumber))
            {
                errors.Add(new IyzicoBuyerFieldError("GsmNumber", Resource.MandatoryField));
                return;
            }

            if (GeneralHelper.IsGsmNumberNotValid(gsmNumber.Trim()))
            {
                errors.Add(new IyzicoBuyerFieldError("GsmNumber", Resource.GsmNumberNotValidMessage));
            }
        }

        private static void ValidateIdentityNumber(string identityNumber, IList<IyzicoBuyerFieldError> errors)
        {
            var digits = identityNumber?.Trim() ?? string.Empty;

            // Exactly 11 characters AND all characters are digits
            if (digits.Length != 11 || !digits.All(char.IsDigit))
            {
                errors.Add(new IyzicoBuyerFieldError("IdentityNumber", Resource.MandatoryField));
            }
        }

        private static bool HasReservedTld(string email)
        {
            var at = email.LastIndexOf('@');
            if (at < 0 || at == email.Length - 1)
            {
                return false;
            }

            var domain = email.Substring(at + 1);
            var lastDot = domain.LastIndexOf('.');
            var tld = lastDot >= 0 ? domain.Substring(lastDot + 1) : domain;

            return ReservedTlds.Contains(tld);
        }
    }

    public sealed class IyzicoBuyerFieldError
    {
        public IyzicoBuyerFieldError(string field, string message)
        {
            Field = field ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Field { get; }
        public string Message { get; }
    }
}
