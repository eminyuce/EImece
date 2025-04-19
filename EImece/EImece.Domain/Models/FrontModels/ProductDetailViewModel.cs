using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
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
        public Product Product { get; set; }

        public ProductComment ProductComment { get; set; }

        public Menu ProductMenu { get; set; }

        public Menu MainPageMenu { get; set; }

        public List<ProductCategoryTreeModel> BreadCrumb { get; set; }

        public Template Template { get; set; }

        public List<Story> RelatedStories { get; set; }

        public List<Product> RelatedProducts { get; set; }

        public ContactUsFormViewModel Contact { get; set; }

        public Setting CargoDescription { get; set; }
        public Setting IsProductPriceEnable { get; set; }
        public Setting IsProductReviewEnable { get; set; }
        public Setting WhatsAppCommunicationLink { get; set; }
        public Setting CompanyName { get; set; }
        public string SeoId { get; set; }

        public ProductDetailViewModel()
        {
            ProductComment = new ProductComment();
        }

        public string GoogleProductSchemaJson
        {
            get
            {
                string plainDescription = HttpUtility.HtmlDecode(GeneralHelper.RemoveHtmlTags(Product.ShortDescription)) ?? "No description available";
                var productComments = Product.ProductComments.IsNotEmpty() ? Product.ProductComments : new List<ProductComment>();

                var schema = new GoogleProductSchema
                {
                    Name = Product.ProductNameStr,
                    Image = new string[] { Product.ImageFullPath(200, 200) },
                    Description = plainDescription,
                    Brand = new GoogleBrand
                    {
                        Name = Product.Brand.Name
                    },
                    Sku = Product.ProductCode,
                    Offers = new GoogleOffer
                    {
                        Url = Product.DetailPageAbsoluteUrl,
                        PriceCurrency = Constants.CURRENCY_TURKISH,
                        Price = Product.PriceWithDiscount.GoogleProductSchema(),
                        Availability = GeneralHelper.GetSchemaAvailability(Product.StateEnum),
                        ItemCondition = "https://schema.org/NewCondition",
                        Seller = new GoogleSeller
                        {
                            Name = CompanyName.SettingValue.ToStr()
                        }
                    }
                };

                if (productComments.IsNotEmpty())
                {
                    schema.AggregateRating = new GoogleAggregateRating
                    {
                        RatingValue = Product.Rating.ToStr("0.0"), // Example: Replace with actual rating logic
                        ReviewCount = productComments.Count.ToStr("0") // Example: Replace with actual review count logic
                    };
                    schema.Review = productComments.Select(r => new GoogleReview
                    {
                        Author = new GoogleAuthor
                        {
                            Name = r.Name // Example: Replace with actual review author
                        },
                        DatePublished = r.UpdatedDate.ToString("yyyy-MM-dd"), // Example: Replace with actual review date
                        ReviewBody = r.Review, // Example: Replace with actual review body
                        Name = r.Subject, // Example: Replace with actual review title
                        ReviewRating = new GoogleReviewRating
                        {
                            RatingValue = r.Rating.ToString(System.Globalization.CultureInfo.InvariantCulture), // Example: Replace with actual rating
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
                // Null kontrolü: Eğer temel verilerden biri null ise boş string dön
                if (WhatsAppCommunicationLink == null || WhatsAppCommunicationLink.SettingValue == null || Product == null)
                {
                    return string.Empty; // ""
                }

                // Şablon
                string whatsAppLinkTemplate = WhatsAppCommunicationLink.SettingValue.ToStr();
                if (string.IsNullOrEmpty(whatsAppLinkTemplate))
                {
                    return string.Empty; // ""
                }
                // Ürün adı (null kontrolü zaten Product için yapıldı, burada sadece ProductNameStr için)
                string detailPageAbsoluteUrl = Product.DetailPageAbsoluteUrl;

                // {product.Name} ile değiştir
                string linkWithProduct = whatsAppLinkTemplate.Replace("{Product.DetailPageAbsoluteUrl}", detailPageAbsoluteUrl);

                // text= kısmını ayır ve escape et
                int textIndex = linkWithProduct.IndexOf("?text=");
                if (textIndex == -1)
                {
                    // Eğer ?text= yoksa, varsayılan bir mesajla devam et
                    string defaultMessage = Uri.EscapeDataString($"Merhaba {detailPageAbsoluteUrl} ile ilgili bilgi almak istiyorum");
                    return $"https://wa.me/905322739101?text={defaultMessage}";
                }

                textIndex += 6; // "?text=" sonrasını al
                string message = linkWithProduct.Substring(textIndex);
                string escapedMessage = Uri.EscapeDataString(message);

                // Nihai linki oluştur
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
                if (Product.ProductComments.IsEmpty())
                {
                    return totalRating;
                }
                var grouped = Product.ProductComments.GroupBy(r => r.Rating)
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
                var product = Product;
                var productSpecs = product.ProductSpecifications.Where(r => !String.IsNullOrEmpty(r.Value)).OrderBy(r => r.Position).ToList();
                var template = Template;
                if (productSpecs.Any() && !string.IsNullOrEmpty(template.TemplateXml))
                {
                    XDocument xdoc = XDocument.Parse(template.TemplateXml);
                    var groups = xdoc.Root.Descendants("group");
                    foreach (var group in groups)
                    {
                        foreach (XElement field in group.Elements())
                        {
                            var name = field.Attribute("name");
                            var unit = field.Attribute("unit");
                            var values = field.Attribute("values");
                            var display = field.Attribute("display");
                            var dbValueObj = productSpecs.FirstOrDefault(r => r.Name.Equals(name.Value, StringComparison.InvariantCultureIgnoreCase));
                            var isValueExist = dbValueObj != null;
                            if (!isValueExist)
                            {
                                continue;
                            }
                            string specsName = "";
                            if (display != null)
                            {
                                specsName = display.Value;
                            }
                            else
                            {
                                specsName = name.Value;
                            }
                            result.Add(new ProductSpecsModel(
                                specsName.ToStr().Trim(),
                               dbValueObj.Value == null ? "" : dbValueObj.Value.ToStr().Trim(),
                               unit == null ? "" : unit.Value.ToStr(), values == null ? "" : values.Value.ToStr()));
                        }
                    }
                }
                return result;
            }
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

        [JsonProperty("availability")]
        public string Availability { get; set; }

        [JsonProperty("itemCondition")]
        public string ItemCondition { get; set; }

        [JsonProperty("seller")]
        public GoogleSeller Seller { get; set; }
    }

    public class GoogleSeller
    {
        [JsonProperty("@type")]
        public string Type { get; set; } = "Organization";

        [JsonProperty("name")]
        public string Name { get; set; }
    }


}