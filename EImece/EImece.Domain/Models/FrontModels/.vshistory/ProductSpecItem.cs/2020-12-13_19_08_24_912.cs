using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    [Serializable]
    public class ProductSpecItem
    {
        public string SpecsName { get; set; }
        public string SpecsValue { get; set; }
        public int Quantity { set; get; }

        public override bool Equals(object obj)
        {
            ProductSpecItem p = (ProductSpecItem)obj;
            if (p == null)
                return false;
            return SpecsName.Equals(p.SpecsName,StringComparison.InvariantCultureIgnoreCase);
        }
    }
    [Serializable]
    public class ProductSpecItemRoot
    {
        public List<ProductSpecItem> selectedTotalSpecs { get; set; }
    }

}
