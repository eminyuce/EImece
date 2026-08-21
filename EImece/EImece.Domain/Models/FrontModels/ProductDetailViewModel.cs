using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductDetailViewModel : ItemListing
    {
        public StorefrontProductDetailDto ProductDto { get; set; }

        public ProductCommentDto ProductComment { get; set; }

        public StorefrontMenuDto ProductMenu { get; set; }

        public StorefrontMenuDto MainPageMenu { get; set; }

        public List<ProductCategoryTreeModel> BreadCrumb { get; set; }

        public TemplateDto Template { get; set; }

        public List<StorefrontStoryCardDto> RelatedStories { get; set; }

        public List<StorefrontProductCardDto> RelatedProducts { get; set; }

        public ContactUsFormViewModel Contact { get; set; }

        public SettingDto CargoDescription { get; set; }
        public SettingDto CargoPrice { get; set; }
        public SettingDto PaymentDetailHtml { get; set; }
        public SettingDto IsProductPriceEnable { get; set; }
        public SettingDto IsProductReviewEnable { get; set; }
        public SettingDto WhatsAppCommunicationLink { get; set; }
        public SettingDto CompanyName { get; set; }
        public string SeoId { get; set; }

        public ProductDetailViewModel()
        {
            ProductComment = new ProductCommentDto();
            RelatedStories = new List<StorefrontStoryCardDto>();
            RelatedProducts = new List<StorefrontProductCardDto>();
        }

        public string GoogleProductSchemaJson
        {
            get
            {
                if (ProductDto == null) return "{}";

                string plainDescription = HttpUtility.HtmlDecode(GeneralHelper.RemoveHtmlTags(ProductDto.ShortDescription)) ?? "No description available";
                var productComments = ProductDto.ProductComments.IsNotEmpty() ? ProductDto.ProductComments : new List<StorefrontProductCommentDto>();
                var productTags = ProductDto.ProductTags;
                var productFiles = ProductDto.ProductFiles;
                List<string> images = new List<string>();
                images.Add(ProductDto.ImageFullPath(200, 200));

                if (productFiles.IsNotEmpty())
                {
                    for (int i = 0; i < productFiles.Count; i++)
                    {
                        var f = productFiles[i];
                        images.Add(f.ImageFullPath(95, 105));
                    }
                }

                string productNameStr = !string.IsNullOrEmpty(ProductDto.NameShort) ? ProductDto.NameShort
                    : (!string.IsNullOrEmpty(ProductDto.NameLong) ? ProductDto.NameLong : ProductDto.Name);

                ProductState stateEnum;
                Enum.TryParse(ProductDto.State, out stateEnum);

                var schema = new GoogleProductSchema
                {
                    Name = productNameStr,
                    Category = ProductDto.ProductCategoryName,
                    Keywords = productTags.IsNotEmpty() ? string.Join(", ", productTags.Select(r => r.Name)) : null,
                    Image = images.ToArray(),
                    Description = plainDescription,
                    Brand = new GoogleBrand
                    {
                        Name = ProductDto.BrandName ?? string.Empty
                    },
                    Sku = ProductDto.ProductCode,
                    Offers = new GoogleOffer
                    {
                        Url = ProductDto.DetailPageAbsoluteUrl,
                        PriceCurrency = Constants.CURRENCY_TURKISH,
                        Price = ProductDto.PriceWithDiscount.GoogleProductSchema(),
                        PriceValidUntil = ProductDto.UpdatedDate.AddMonths(3).ToString("yyyy-MM-dd"),
                        Availability = GeneralHelper.GetSchemaAvailability(stateEnum),
                        ItemCondition = "https://schema.org/NewCondition",
                        Seller = new GoogleSeller
                        {
                            Name = CompanyName.SettingValue.ToStr()
                        },
                        HasMerchantReturnPolicy = new GoogleReturnPolicy(),
                        ShippingDetails = new GoogleShippingDetails
                        {
                            ShippingRate = new GoogleShippingRate
                            {
                                Value = CargoPrice.SettingValue.ToDecimal().GoogleProductSchema(),
                                Currency = Constants.CURRENCY_TURKISH
                            },
                            ShippingDestination = new GoogleShippingDestination
                            {
                                AddressCountry = "TR"
                            }
                        }
                    }
                };

                if (productComments.IsNotEmpty())
                {
                    schema.AggregateRating = new GoogleAggregateRating
                    {
                        RatingValue = ProductDto.Rating.ToStr("0.0"),
                        ReviewCount = productComments.Count.ToStr("0")
                    };
                    schema.Review = productComments.Select(r => new GoogleReview
                    {
                        Author = new GoogleAuthor
                        {
                            Name = r.Name
                        },
                        DatePublished = r.CreatedDate.ToString("yyyy-MM-dd"),
                        ReviewBody = r.Comment,
                        Name = r.Name,
                        ReviewRating = new GoogleReviewRating
                        {
                            RatingValue = r.Rating.ToString(System.Globalization.CultureInfo.InvariantCulture),
                            BestRating = "5",
                            WorstRating = "1"
                        }
                    }).ToList();
                }

                return JsonConvert.SerializeObject(schema, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });
            }
        }


        public string WhatsAppCommunicationLinkGenerateScript
        {
            get
            {
                if (WhatsAppCommunicationLink == null || WhatsAppCommunicationLink.SettingValue == null || ProductDto == null)
                {
                    return string.Empty;
                }

                string whatsAppLinkTemplate = WhatsAppCommunicationLink.SettingValue.ToStr();
                if (string.IsNullOrEmpty(whatsAppLinkTemplate))
                {
                    return string.Empty;
                }
                string detailPageAbsoluteUrl = ProductDto.DetailPageAbsoluteUrl;

                string linkWithProduct = whatsAppLinkTemplate.Replace("{Product.DetailPageAbsoluteUrl}", detailPageAbsoluteUrl);

                int textIndex = linkWithProduct.IndexOf("?text=");
                if (textIndex == -1)
                {
                    string defaultMessage = Uri.EscapeDataString($"Merhaba {detailPageAbsoluteUrl} ile ilgili bilgi almak istiyorum");
                    return $"https://wa.me/905322739101?text={defaultMessage}";
                }

                textIndex += 6;
                string message = linkWithProduct.Substring(textIndex);
                string escapedMessage = Uri.EscapeDataString(message);

                string finalLink = linkWithProduct.Substring(0, textIndex) + escapedMessage;

                return finalLink;
            }
        }

        public Dictionary<string, string> SocialMediaLinks { get; set; }

        public string AverageRating
        {
            get
            {
                if (TotalRating.Count > 0)
                {
                    double totalRatingCount = (double)TotalRating.Sum(r => r.Key * r.Value.Count);
                    int totalCount = TotalRating.Sum(r => r.Value.Count);
                    return string.Format("{0:0.00}", totalRatingCount / totalCount);
                }
                else
                {
                    return "0";
                }
            }
        }

        public Dictionary<int, TotalRating> TotalRating
        {
            get
            {
                var totalRating = new Dictionary<int, TotalRating>();
                if (ProductDto == null || ProductDto.ProductComments.IsEmpty())
                {
                    return totalRating;
                }
                var grouped = ProductDto.ProductComments.GroupBy(r => r.Rating)
                     .OrderByDescending(grp => grp.Key)
                .Select((grp, i) => new
                {
                    Rating = grp.Key,
                    Count = grp.Count()
                })
                .ToList();
                double total = grouped.Sum(r => r.Count);
                totalRating = grouped.ToDictionary(r => r.Rating, r => new TotalRating(r.Count, (int)Math.Round(r.Count * 100 / total)));
                return totalRating;
            }
        }

        public List<ProductSpecsModel> ProdSpecs
        {
            get
            {
                var result = new List<ProductSpecsModel>();
                if (ProductDto?.ProductSpecifications == null)
                {
                    return result;
                }

                var productSpecs = ProductDto.ProductSpecifications.OrderBy(r => r.Order).ToList();
                var template = Template;
                if (!productSpecs.Any() || template == null || string.IsNullOrEmpty(template.TemplateXml))
                {
                    return result;
                }

                XDocument xdoc = XDocument.Parse(template.TemplateXml);
                var groups = xdoc.Root.Descendants()
                    .Where(e => e.Name.LocalName.Equals("group", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (groups.Any())
                {
                    foreach (var group in groups)
                    {
                        var groupNameAttr = group.Attribute("name") ?? group.FirstAttribute;
                        var groupName = groupNameAttr != null ? groupNameAttr.Value.ToStr().Trim() : string.Empty;
                        AppendSpecFields(result, productSpecs, group.Elements(), groupName);
                    }
                }
                else
                {
                    var fields = xdoc.Descendants().Where(e =>
                        e.Attribute("name") != null
                        && !e.Name.LocalName.Equals("group", StringComparison.OrdinalIgnoreCase)
                        && !e.Name.LocalName.Equals("fields", StringComparison.OrdinalIgnoreCase)
                        && !e.Name.LocalName.Equals("template", StringComparison.OrdinalIgnoreCase)
                        && !e.Name.LocalName.Equals("component", StringComparison.OrdinalIgnoreCase));
                    AppendSpecFields(result, productSpecs, fields, string.Empty);
                }

                return result;
            }
        }

        /// <summary>
        /// Specs grouped by template &lt;group name&gt; for storefront tables/sections.
        /// </summary>
        public List<ProductSpecsGroupModel> GetProdSpecsGroups()
        {
            return ProdSpecs
                .GroupBy(s => s.groupName ?? string.Empty, StringComparer.Ordinal)
                .Select(g => new ProductSpecsGroupModel(g.Key, g.ToList()))
                .Where(g => g.Items != null && g.Items.Any())
                .ToList();
        }

        private static void AppendSpecFields(
            List<ProductSpecsModel> result,
            List<StorefrontProductSpecificationDto> productSpecs,
            IEnumerable<XElement> fields,
            string groupName)
        {
            foreach (XElement field in fields)
            {
                var name = field.Attribute("name");
                if (name == null || string.IsNullOrWhiteSpace(name.Value))
                {
                    continue;
                }

                var unit = field.Attribute("unit");
                var values = field.Attribute("values");
                var display = field.Attribute("display");
                var dbValueObj = productSpecs.FirstOrDefault(r =>
                    r.Name != null && r.Name.Equals(name.Value, StringComparison.InvariantCultureIgnoreCase));
                if (dbValueObj == null)
                {
                    continue;
                }

                var isCheckbox = ProductSpecificationValueHelper.IsCheckboxField(field);
                var rawValue = dbValueObj.Value == null ? "" : dbValueObj.Value.ToStr().Trim();

                if (ShouldOmitEmptyNonCheckboxSpec(isCheckbox, rawValue))
                {
                    continue;
                }

                result.Add(CreateSpecModel(new SpecModelArgs
                {
                    Field = field,
                    Name = name,
                    Unit = unit,
                    Values = values,
                    Display = display,
                    RawValue = rawValue,
                    IsCheckbox = isCheckbox,
                    GroupName = groupName
                }));
            }
        }

        private static bool ShouldOmitEmptyNonCheckboxSpec(bool isCheckbox, string rawValue)
        {
            return !isCheckbox && string.IsNullOrEmpty(rawValue);
        }

        private static ProductSpecsModel CreateSpecModel(SpecModelArgs args)
        {
            string specsName = args.Display != null ? args.Display.Value : args.Name.Value;
            string displayValue = ProductSpecificationValueHelper.FormatSpecDisplayValue(args.Field, args.RawValue);
            string displayUnit = ResolveSpecDisplayUnit(args.IsCheckbox, args.Unit);
            return new ProductSpecsModel(
                specsName.ToStr().Trim(),
                displayValue,
                displayUnit,
                args.Values == null ? "" : args.Values.Value.ToStr(),
                args.GroupName);
        }

        private sealed class SpecModelArgs
        {
            public XElement Field { get; set; }
            public XAttribute Name { get; set; }
            public XAttribute Unit { get; set; }
            public XAttribute Values { get; set; }
            public XAttribute Display { get; set; }
            public string RawValue { get; set; }
            public bool IsCheckbox { get; set; }
            public string GroupName { get; set; }
        }

        private static string ResolveSpecDisplayUnit(bool isCheckbox, XAttribute unit)
        {
            if (isCheckbox || unit == null)
            {
                return "";
            }

            return unit.Value.ToStr();
        }
    }

    public class TotalRating
    {
        public int Count { get; set; }
        public int Percentage { get; set; }

        public TotalRating(int count, int percentage)
        {
            this.Count = count;
            this.Percentage = percentage;
        }
    }

    public class GoogleProductSchema
    {
        [JsonProperty("@context")]
        public string Context { get; set; } = "https://schema.org";

        [JsonProperty("@type")]
        public string Type { get; set; } = "Product";

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("keywords")]
        public string Keywords { get; set; }

        [JsonProperty("image")]
        public string [] Image { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("brand")]
        public GoogleBrand Brand { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("offers")]
        public GoogleOffer Offers { get; set; }

        [JsonProperty("aggregateRating")]
        public GoogleAggregateRating AggregateRating { get; set; }

        [JsonProperty("review")]
        public List<GoogleReview> Review { get; set; }
    }

    public class GoogleAggregateRating
    {
        [JsonProperty("@type")]
        public string Type { get; set; } = "AggregateRating";

        [JsonProperty("ratingValue")]
        public string RatingValue { get; set; }

        [JsonProperty("reviewCount")]
        public string ReviewCount { get; set; }
    }

    public class GoogleReview
    {
        [JsonProperty("@type")]
        public string Type { get; set; } = "Review";

        [JsonProperty("author")]
        public GoogleAuthor Author { get; set; }

        [JsonProperty("datePublished")]
        public string DatePublished { get; set; }

        [JsonProperty("reviewBody")]
        public string ReviewBody { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("reviewRating")]
        public GoogleReviewRating ReviewRating { get; set; }
    }

    public class GoogleAuthor
    {
        [JsonProperty("@type")]
        public string Type { get; set; } = "Person";

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class GoogleReviewRating
    {
        [JsonProperty("@type")]
        public string Type { get; set; } = "Rating";

        [JsonProperty("ratingValue")]
        public string RatingValue { get; set; }

        [JsonProperty("bestRating")]
        public string BestRating { get; set; }

        [JsonProperty("worstRating")]
        public string WorstRating { get; set; }
    }

    public class GoogleBrand
    {
        [JsonProperty("@type")]
        public string Type { get; set; } = "Brand";

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class GoogleOffer
    {
        [JsonProperty("@type")]
        public string Type { get; set; } = "Offer";

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("priceCurrency")]
        public string PriceCurrency { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("priceValidUntil")]
        public string PriceValidUntil { get; set; } // NEW

        [JsonProperty("availability")]
        public string Availability { get; set; }

        [JsonProperty("itemCondition")]
        public string ItemCondition { get; set; }

        [JsonProperty("seller")]
        public GoogleSeller Seller { get; set; }

        [JsonProperty("hasMerchantReturnPolicy")]
        public GoogleReturnPolicy HasMerchantReturnPolicy { get; set; } // NEW

        [JsonProperty("shippingDetails")]
        public GoogleShippingDetails ShippingDetails { get; set; } // NEW
    }

    public class GoogleSeller
    {
        [JsonProperty("@type")]
        public string Type { get; set; } = "Organization";

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class GoogleReturnPolicy
    {
        [JsonProperty("@type")]
        public string Type { get; set; } = "MerchantReturnPolicy";

        [JsonProperty("returnPolicyCategory")]
        public string ReturnPolicyCategory { get; set; } = "https://schema.org/Returnable";

        [JsonProperty("merchantReturnDays")]
        public int MerchantReturnDays { get; set; } = 14;

        [JsonProperty("returnMethod")]
        public string ReturnMethod { get; set; } = "https://schema.org/InStoreReturn";

        [JsonProperty("returnFees")]
        public string ReturnFees { get; set; } = "https://schema.org/FreeReturn";
    }

    public class GoogleShippingDetails
    {
        [JsonProperty("@type")]
        public string Type { get; set; } = "OfferShippingDetails";

        [JsonProperty("shippingRate")]
        public GoogleShippingRate ShippingRate { get; set; }

        [JsonProperty("shippingDestination")]
        public GoogleShippingDestination ShippingDestination { get; set; }
    }

    public class GoogleShippingRate
    {
        [JsonProperty("@type")]
        public string Type { get; set; } = "MonetaryAmount";

        [JsonProperty("value")]
        public string Value { get; set; } = "0.00";

        [JsonProperty("currency")]
        public string Currency { get; set; } = "TRY";
    }

    public class GoogleShippingDestination
    {
        [JsonProperty("@type")]
        public string Type { get; set; } = "DefinedRegion";

        [JsonProperty("addressCountry")]
        public string AddressCountry { get; set; } = "TR";
    }


}