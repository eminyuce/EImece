using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public  class ProductCategoryViewModel
    {
        public ProductCategory ProductCategory { get; set; }

        public  List<ProductCategoryTreeModel> ProductCategoryTree { get; set; }
    }
}
