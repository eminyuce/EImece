using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductCategoryTreeModel
    {
        public ProductCategory ProductCategory { get; set; }
        public int ProductCount { get; set; }

        public List<ProductCategoryTreeModel> Childrens { get; set; }

        public String Text
        {
            get
            {
                if(ProductCount > 0)
                {
                    return String.Format("{0} ({1})", ProductCategory.Name, ProductCount);
                }
                else
                {
                    return String.Format("{0}", ProductCategory.Name);
                }
            }
        }

    }
}
