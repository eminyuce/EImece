using EImece.Domain.Models.AdminModels;
using EImece.Domain.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        private static readonly Regex ForeachRegex = new Regex(
            @"@foreach\s*\(\s*var\s+(\w+)\s+in\s+@?Model\.([A-Za-z_][A-Za-z0-9_]*)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly HashSet<string> MethodNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ToString", "CurrencySign", "ToDecimal", "Equals", "ToInt", "ToDouble",
            "Trim", "ToLower", "ToUpper", "Contains", "Replace", "Substring"
        };

        public static MailTemplateModelUsage Analyze(params string[] sources)
        {
            var usage = new MailTemplateModelUsage();
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (sources == null)
            {
                return usage;
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
                    var path = StripMethodNames(match.Groups[1].Value);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        paths.Add(path);
                    }
                }

                foreach (Match match in IndexerPropertyRegex.Matches(decoded))
                {
                    var path = StripMethodNames(match.Groups[1].Value);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        paths.Add(path);
                    }
                }

                foreach (Match loop in ForeachRegex.Matches(decoded))
                {
                    var loopVar = loop.Groups[1].Value;
                    var collectionPath = loop.Groups[2].Value;
                    if (string.IsNullOrWhiteSpace(collectionPath))
                    {
                        continue;
                    }

                    paths.Add(collectionPath);
                    var itemRegex = new Regex(
                        @"@" + Regex.Escape(loopVar) + @"\.([A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*)",
                        RegexOptions.IgnoreCase);
                    var itemFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (Match itemMatch in itemRegex.Matches(decoded))
                    {
                        var field = StripMethodNames(itemMatch.Groups[1].Value);
                        if (!string.IsNullOrWhiteSpace(field))
                        {
                            itemFields.Add(field);
                        }
                    }

                    List<string> existing;
                    if (!usage.CollectionItemPaths.TryGetValue(collectionPath, out existing))
                    {
                        existing = new List<string>();
                        usage.CollectionItemPaths[collectionPath] = existing;
                    }

                    foreach (var field in itemFields.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                    {
                        if (!existing.Contains(field, StringComparer.OrdinalIgnoreCase))
                        {
                            existing.Add(field);
                        }
                    }
                }
            }

            usage.PropertyPaths = paths
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();
            return usage;
        }

        public static List<string> ExtractPropertyPaths(params string[] sources)
        {
            return Analyze(sources).PropertyPaths;
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

            if (ContainsAny(name, "count", "qty", "quantity", "number", "amount", "price", "total", "position", "fee", "rate"))
            {
                return MailTemplateValueKind.Number;
            }

            if (name.EndsWith("products", StringComparison.Ordinal)
                || name.EndsWith("items", StringComparison.Ordinal)
                || name.EndsWith("list", StringComparison.Ordinal)
                || name.EndsWith("collection", StringComparison.Ordinal))
            {
                return MailTemplateValueKind.Collection;
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
                case MailTemplateValueKind.Collection:
                    return GenerateCollectionSample(propertyPath, null, ctx);
                default:
                    if (ContainsAny(last.ToLowerInvariant(), "company", "sirket", "şirket"))
                    {
                        return ctx.CompanyName;
                    }
                    if (ContainsAny(last.ToLowerInvariant(), "address", "adres", "street"))
                    {
                        return ctx.CompanyAddress;
                    }
                    if (ContainsAny(last.ToLowerInvariant(), "name", "adsoyad", "fullname"))
                    {
                        return "Test Kullanıcı";
                    }
                    if (ContainsAny(last.ToLowerInvariant(), "message", "comment", "note"))
                    {
                        return "Bu bir test mesajıdır. Şablon görselini doğrulamak için gönderildi.";
                    }
                    if (ContainsAny(last.ToLowerInvariant(), "city", "sehir", "şehir"))
                    {
                        return "İstanbul";
                    }
                    if (ContainsAny(last.ToLowerInvariant(), "country", "ulke", "ülke"))
                    {
                        return "Türkiye";
                    }
                    if (ContainsAny(last.ToLowerInvariant(), "district", "ilce", "ilçe"))
                    {
                        return "Kadıköy";
                    }
                    if (ContainsAny(last.ToLowerInvariant(), "zip", "posta"))
                    {
                        return "34710";
                    }
                    if (ContainsAny(last.ToLowerInvariant(), "ip"))
                    {
                        return "127.0.0.1";
                    }
                    return "Örnek " + last;
            }
        }

        public static List<MailTemplateModelProperty> BuildProperties(
            IEnumerable<string> propertyPaths,
            MailTemplateDummyDataContext context = null)
        {
            return BuildProperties(propertyPaths, context, null);
        }

        public static List<MailTemplateModelProperty> BuildProperties(
            IEnumerable<string> propertyPaths,
            MailTemplateDummyDataContext context,
            IDictionary<string, List<string>> collectionItemPaths)
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
                List<string> itemFields = null;
                if (collectionItemPaths != null && collectionItemPaths.TryGetValue(path, out itemFields))
                {
                    kind = MailTemplateValueKind.Collection;
                }

                string sample;
                if (kind == MailTemplateValueKind.Collection)
                {
                    sample = GenerateCollectionSample(path, itemFields, ctx);
                }
                else
                {
                    sample = GenerateSampleValue(path, ctx);
                }

                result.Add(new MailTemplateModelProperty
                {
                    Path = path,
                    ValueKind = kind.ToString(),
                    SampleValue = sample
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

        public static MailTemplateTestRenderResult FromRazorResult(string subject, RazorRenderResult subjectRender, RazorRenderResult bodyRender)
        {
            var result = new MailTemplateTestRenderResult();
            string error;
            if (TryGetRenderError(subjectRender, out error) || TryGetRenderError(bodyRender, out error))
            {
                result.Success = false;
                result.ErrorMessage = error;
                return result;
            }

            result.Success = true;
            result.Subject = subjectRender != null && !string.IsNullOrEmpty(subjectRender.Result)
                ? subjectRender.Result
                : (subject ?? string.Empty);
            result.Body = bodyRender != null ? (bodyRender.Result ?? string.Empty) : string.Empty;
            return result;
        }

        public static bool TryGetRenderError(RazorRenderResult render, out string error)
        {
            error = null;
            if (render == null)
            {
                return false;
            }

            if (render.templateCompilationException != null)
            {
                error = FormatCompilationError(render.templateCompilationException);
                return true;
            }

            if (render.GeneralError != null)
            {
                error = render.GeneralError.ToFormattedString();
                return true;
            }

            return false;
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
                case "fullname":
                    return "Test Kullanıcı";
                case "ordernumber":
                    return "ORD-1001";
                case "emailsubject":
                    return "Test sipariş bildirimi";
                case "ipaddress":
                    return "127.0.0.1";
                case "message":
                    return "Bu bir test mesajıdır. Şablon görselini doğrulamak için gönderildi.";
                case "reasons":
                    return "Ürün hakkında bilgi";
                case "captcha":
                    return "TEST";
                case "itemid":
                    return "1001";
                case "itemtype":
                    return "Product";
                case "installment":
                    return "1";
                case "installmentdescription":
                    return "Tek çekim";
                case "cardfamily":
                    return "Bonus";
                case "cardtype":
                    return "CREDIT_CARD";
                case "cardassociation":
                    return "VISA";
                case "lastfourdigits":
                    return "4242";
                case "paymentstatus":
                    return "SUCCESS";
                case "coupon":
                    return string.Empty;
                case "ordercomments":
                    return "Test sipariş notu";
                case "cargoprice":
                    return "25.00";
                case "paidpricedecimal":
                    return "299.90";
                case "coupondiscount":
                    return "0";
                case "productname":
                    return "Örnek Ürün";
                default:
                    return null;
            }
        }

        private static string GenerateCollectionSample(
            string propertyPath,
            IList<string> itemFields,
            MailTemplateDummyDataContext ctx)
        {
            var fields = (itemFields != null && itemFields.Count > 0)
                ? itemFields
                : new List<string> { "ProductName", "Quantity", "Price", "TotalPrice" };

            var items = new List<Dictionary<string, string>>();
            for (var n = 1; n <= 2; n++)
            {
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var field in fields)
                {
                    var value = GenerateSampleValue(field, ctx);
                    if (field.Equals("ProductName", StringComparison.OrdinalIgnoreCase)
                        || field.Equals("Name", StringComparison.OrdinalIgnoreCase))
                    {
                        value = "Örnek Ürün " + n;
                    }
                    else if (field.Equals("Quantity", StringComparison.OrdinalIgnoreCase))
                    {
                        value = n.ToString(CultureInfo.InvariantCulture);
                    }
                    else if (ContainsAny(field.ToLowerInvariant(), "price", "total"))
                    {
                        value = (n * 149.90m).ToString("0.00", CultureInfo.InvariantCulture);
                    }

                    row[field] = value;
                }

                items.Add(row);
            }

            return JsonConvert.SerializeObject(items, Formatting.Indented);
        }

        private static object CoerceValue(string propertyName, string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var trimmed = value.Trim();
            if (trimmed.StartsWith("[", StringComparison.Ordinal) || trimmed.StartsWith("{", StringComparison.Ordinal))
            {
                object parsed;
                if (TryParseJsonValue(propertyName, trimmed, out parsed))
                {
                    return parsed;
                }
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
                return new DummyMailValue(ParseDecimalOrDefault(value));
            }

            if (kind == MailTemplateValueKind.Date)
            {
                DateTime date;
                if (DateTime.TryParse(value, CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.None, out date)
                    || DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return new DummyMailValue(date);
                }

                return new DummyMailValue(DateTime.Now);
            }

            return value;
        }

        private static bool TryParseJsonValue(string propertyName, string json, out object parsed)
        {
            parsed = null;
            try
            {
                var token = JToken.Parse(json);
                parsed = ConvertJsonToken(propertyName, token);
                return parsed != null;
            }
            catch (JsonReaderException)
            {
                return false;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static object ConvertJsonToken(string propertyName, JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return string.Empty;
            }

            var array = token as JArray;
            if (array != null)
            {
                var list = new List<DynamicMailTemplateModel>();
                foreach (var item in array)
                {
                    var child = ConvertJsonToken(propertyName, item) as DynamicMailTemplateModel;
                    list.Add(child ?? WrapScalarAsModel(propertyName, item));
                }

                return list;
            }

            var obj = token as JObject;
            if (obj != null)
            {
                var model = new DynamicMailTemplateModel();
                foreach (var prop in obj.Properties())
                {
                    model.SetValue(prop.Name, ConvertJsonToken(prop.Name, prop.Value));
                }

                return model;
            }

            return CoerceLeafFromJson(propertyName, token);
        }

        private static DynamicMailTemplateModel WrapScalarAsModel(string propertyName, JToken token)
        {
            var model = new DynamicMailTemplateModel();
            model.SetValue(propertyName, CoerceLeafFromJson(propertyName, token));
            return model;
        }

        private static object CoerceLeafFromJson(string propertyName, JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return string.Empty;
            }

            if (token.Type == JTokenType.Object || token.Type == JTokenType.Array)
            {
                return ConvertJsonToken(propertyName, token);
            }

            return CoerceValue(propertyName, token.ToString());
        }

        private static decimal ParseDecimalOrDefault(string value)
        {
            decimal d;
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out d)
                || decimal.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out d))
            {
                return d;
            }

            return 1m;
        }

        private static string StripMethodNames(string propertyPath)
        {
            if (string.IsNullOrWhiteSpace(propertyPath))
            {
                return string.Empty;
            }

            var parts = propertyPath.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            while (parts.Count > 1 && MethodNames.Contains(parts[parts.Count - 1]))
            {
                parts.RemoveAt(parts.Count - 1);
            }

            return string.Join(".", parts);
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
