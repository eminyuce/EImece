using EImece.Domain.Entities;
using EImece.Domain.Helpers;
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

        public ProductDetailViewModel()
        {
            ProductComment = new ProductComment();
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
}