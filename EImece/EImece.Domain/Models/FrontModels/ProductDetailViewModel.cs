using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public ProductDetailViewModel()
        {
            ProductComment = new ProductComment();
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
}