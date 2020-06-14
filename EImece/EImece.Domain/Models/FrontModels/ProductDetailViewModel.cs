using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }

        public Menu ProductMenu { get; set; }

        public Menu MainPageMenu { get; set; }

        public List<ProductCategoryTreeModel> BreadCrumb { get; set; }

        public Template Template { get; set; }

        public List<Story> RelatedStories { get; set; }

        public List<Product> RelatedProducts { get; set; }

        public ContactUsFormViewModel Contact { get; set; }
    }
}