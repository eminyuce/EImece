using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }

        public List<ProductCategoryTreeModel> BreadCrumb { get; set; }

        public Template Template { get; set; }

        public List<Story> RelatedStories { get; set; }
    }
}
