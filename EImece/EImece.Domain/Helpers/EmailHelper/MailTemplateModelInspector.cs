using EImece.Domain.Models.AdminModels;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace EImece.Domain.Helpers.EmailHelper
{
    /// <summary>
    /// Inspects Razor email templates for @Model properties, generates dummy values,
    /// and renders the template against a dynamic model. Property names are discovered
    /// from the template so new templates do not need hardcoded model classes.
    /// </summary>
    public static class MailTemplateModelInspector
    {
        private static readonly Regex DotPropertyRegex = new Regex(
            @"(?<!@)@Model\.([A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*)",
            RegexOptions.Compiled);

        private static readonly Regex IndexerPropertyRegex = new Regex(
            @"(?<!@)@Model\[\s*[""']([^""']+)[""']\s*\]",
            RegexOptions.Compiled);

        public static List<string> ExtractPropertyPaths(params string[] sources)
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (sources == null)
            {
                return new List<string>();
            }

            foreach (var source in sources)
            {
                if (string.IsNullOrWhiteSpace(source))
                {
                    continue;
                }

                var decoded = WebUtility.HtmlDecode(source) ?? source;
                foreach (Match match in DotPropertyRegex.Matches(decoded))
                {
                    var path = match.Groups[1].Value;
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        paths.Add(path);
                    }
                }

                foreach (Match match in IndexerPropertyRegex.Matches(decoded))
                {
                    var path = match.Groups[1].Value;
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        paths.Add(path);
                    }
                }
            }

            return paths
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static MailTemplateValueKind InferValueKind(string propertyPath)
        {
            var name = GetLastSegment(propertyPath).ToLowerInvariant();

            if (ContainsAny(name, "email", "eposta") || name.EndsWith("mail", StringComparison.Ordinal))
            {
                return MailTemplateValueKind.Email;
            }

            if (ContainsAny(name, "icon", "logo", "image", "img", "photo", "picture"))
            {
                return MailTemplateValueKind.ImageUrl;
            }

            if (ContainsAny(name, "url", "link", "href", "uri") || name.EndsWith("page", StringComparison.Ordinal) || name.EndsWith("src", StringComparison.Ordinal))
            {
                return MailTemplateValueKind.Url;
            }

            if (ContainsAny(name, "phone", "tel", "mobile", "gsm"))
            {
                return MailTemplateValueKind.Phone;
            }

            if (ContainsAny(name, "date", "time"))
            {
                return MailTemplateValueKind.Date;
            }

            if (name.StartsWith("is", StringComparison.Ordinal)
                || name.StartsWith("has", StringComparison.Ordinal)
                || name.StartsWith("enable", StringComparison.Ordinal)
                || name == "active")
            {
                return MailTemplateValueKind.Boolean;
            }

            if (ContainsAny(name, "count", "qty", "quantity", "number", "amount", "price", "total", "position"))
            {
                return MailTemplateValueKind.Number;
            }

            return MailTemplateValueKind.String;
        }

        public static string GenerateSampleValue(string propertyPath, MailTemplateDummyDataContext context = null)
        {
            var ctx = NormalizeContext(context);
            var known = TryKnownValue(propertyPath, ctx);
            if (known != null)
            {
                return known;
            }

            var kind = InferValueKind(propertyPath);
            var last = GetLastSegment(propertyPath);
            switch (kind)
            {
                case MailTemplateValueKind.Email:
                    return ctx.RecipientEmail;
                case MailTemplateValueKind.ImageUrl:
                    return ctx.LogoUrl;
                case MailTemplateValueKind.Url:
                    return ctx.BaseUrl.TrimEnd('/') + "/" + ToSlug(last);
                case MailTemplateValueKind.Phone:
                    return ctx.CompanyPhone;
                case MailTemplateValueKind.Date:
                    return DateTime.Now.ToString("dd.MM.yyyy HH:mm", CultureInfo.GetCultureInfo("tr-TR"));
                case MailTemplateValueKind.Boolean:
                    return "True";
                case MailTemplateValueKind.Number:
                    return "1";
                default:
                    if (ContainsAny(last.ToLowerInvariant(), "company", "sirket", "şirket"))
                    {
                        return ctx.CompanyName;
                    }
                    if (ContainsAny(last.ToLowerInvariant(), "address", "adres"))
                    {
                        return ctx.CompanyAddress;
                    }
                    if (ContainsAny(last.ToLowerInvariant(), "name", "adsoyad"))
                    {
                        return "Test Kullanıcı";
                    }
                    return "Örnek " + last;
            }
        }

        public static List<MailTemplateModelProperty> BuildProperties(
            IEnumerable<string> propertyPaths,
            MailTemplateDummyDataContext context = null)
        {
            var ctx = NormalizeContext(context);
            var result = new List<MailTemplateModelProperty>();
            if (propertyPaths == null)
            {
                return result;
            }

            foreach (var path in propertyPaths)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                var kind = InferValueKind(path);
                result.Add(new MailTemplateModelProperty
                {
                    Path = path,
                    ValueKind = kind.ToString(),
                    SampleValue = GenerateSampleValue(path, ctx)
                });
            }

            return result;
        }

        public static DynamicMailTemplateModel BuildDynamicModel(IDictionary<string, string> values)
        {
            var root = new DynamicMailTemplateModel();
            if (values == null)
            {
                return root;
            }

            foreach (var pair in values)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                {
                    continue;
                }

                var parts = pair.Key.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                {
                    continue;
                }

                var current = root;
                for (var i = 0; i < parts.Length - 1; i++)
                {
                    current = current.GetOrCreateChild(parts[i]);
                }

                current.SetValue(parts[parts.Length - 1], CoerceValue(parts[parts.Length - 1], pair.Value));
            }

            return root;
        }

        public static MailTemplateTestRenderResult Render(string subject, string body, IDictionary<string, string> modelData)
        {
            var result = new MailTemplateTestRenderResult();
            var model = BuildDynamicModel(modelData);

            try
            {
                result.Subject = RenderFragment(subject, model, "subject");
                result.Body = RenderFragment(body, model, "body");
                result.Success = true;
            }
            catch (TemplateCompilationException ex)
            {
                result.Success = false;
                result.ErrorMessage = FormatCompilationError(ex);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.ToFormattedString();
            }

            return result;
        }

        public static string RenderFragment(string template, object model, string keyPrefix)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return string.Empty;
            }

            var decoded = WebUtility.HtmlDecode(template) ?? template;
            if (decoded.IndexOf("@", StringComparison.Ordinal) < 0)
            {
                return decoded;
            }

            var key = "mailtest_" + (keyPrefix ?? "tpl") + "_" + GeneralHelper.GetHashString(decoded);
            return Engine.Razor.RunCompile(decoded, key, null, model ?? new DynamicMailTemplateModel());
        }

        private static string FormatCompilationError(TemplateCompilationException ex)
        {
            if (ex != null && ex.CompilerErrors != null)
            {
                var first = ex.CompilerErrors.FirstOrDefault();
                if (first != null && !string.IsNullOrWhiteSpace(first.ErrorText))
                {
                    return "Şablon derlenemedi: " + first.ErrorText;
                }
            }

            return "Şablon derlenemedi: " + (ex != null ? ex.Message : "bilinmeyen hata");
        }

        private static MailTemplateDummyDataContext NormalizeContext(MailTemplateDummyDataContext context)
        {
            var defaults = MailTemplateDummyDataContext.CreateDefaults();
            if (context == null)
            {
                return defaults;
            }

            context.BaseUrl = FirstNonEmpty(context.BaseUrl, defaults.BaseUrl);
            context.CompanyName = FirstNonEmpty(context.CompanyName, defaults.CompanyName);
            context.CompanyEmail = FirstNonEmpty(context.CompanyEmail, defaults.CompanyEmail);
            context.CompanyAddress = FirstNonEmpty(context.CompanyAddress, defaults.CompanyAddress);
            context.CompanyPhone = FirstNonEmpty(context.CompanyPhone, defaults.CompanyPhone);
            context.RecipientEmail = FirstNonEmpty(context.RecipientEmail, defaults.RecipientEmail);
            context.LogoUrl = FirstNonEmpty(context.LogoUrl, context.BaseUrl.TrimEnd('/') + Constants.LogoImagePath);
            return context;
        }

        private static string TryKnownValue(string propertyPath, MailTemplateDummyDataContext ctx)
        {
            var last = GetLastSegment(propertyPath);
            var key = last.ToLowerInvariant();
            switch (key)
            {
                case "websiteiconurl":
                case "imglogosrc":
                case "logourl":
                    return ctx.LogoUrl;
                case "email":
                    return ctx.RecipientEmail;
                case "companyname":
                    return ctx.CompanyName;
                case "forgotpasswordlink":
                    return ctx.BaseUrl.TrimEnd('/') + "/account/resetpassword?code=TEST-RESET-TOKEN";
                case "callbackurl":
                    return ctx.BaseUrl.TrimEnd('/') + "/account/confirmemail?code=TEST-CONFIRM-TOKEN";
                case "companypagelink":
                case "baseurl":
                case "companywebsiteurl":
                case "productpagelink":
                    return ctx.BaseUrl;
                case "companyemailaddress":
                case "websitecompanyemailaddress":
                    return ctx.CompanyEmail;
                case "companyaddress":
                    return ctx.CompanyAddress;
                case "companyphonenumber":
                    return ctx.CompanyPhone;
                case "adminpanelurl":
                case "adminpageurl":
                    return ctx.BaseUrl.TrimEnd('/') + "/account/adminlogin/";
                case "name":
                case "customername":
                    return "Test Kullanıcı";
                case "ordernumber":
                    return "ORD-1001";
                default:
                    return null;
            }
        }

        private static object CoerceValue(string propertyName, string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var kind = InferValueKind(propertyName);
            if (kind == MailTemplateValueKind.Boolean)
            {
                bool parsed;
                if (bool.TryParse(value, out parsed))
                {
                    return parsed;
                }
            }

            if (kind == MailTemplateValueKind.Number)
            {
                int i;
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out i))
                {
                    return i;
                }

                decimal d;
                if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                {
                    return d;
                }
            }

            return value;
        }

        private static string GetLastSegment(string propertyPath)
        {
            if (string.IsNullOrWhiteSpace(propertyPath))
            {
                return string.Empty;
            }

            var parts = propertyPath.Split('.');
            return parts[parts.Length - 1];
        }

        private static bool ContainsAny(string value, params string[] tokens)
        {
            if (string.IsNullOrEmpty(value) || tokens == null)
            {
                return false;
            }

            foreach (var token in tokens)
            {
                if (value.IndexOf(token, StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string FirstNonEmpty(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static string ToSlug(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "ornek";
            }

            return value.Trim().ToLowerInvariant();
        }
    }
}
