using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductSpecItem
    {
        private String SpecsName { get; set; }
        private String SpecsValue { get; set; }
    }
    public class ProductSpecItemRoot
    {
        public List<ProductSpecItem> selectedTotalSpecs { get; set; }
    }

}
