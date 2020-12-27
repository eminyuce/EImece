using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    [Serializable]
    public class ProductSpecItem
    {
        public string SpecsName { get; set; }
        public string SpecsValue { get; set; }

        public override bool Equals(object obj)
        {
            ProductSpecItem p = (ProductSpecItem)obj;
            if (p == null)
                return false;
            return SpecsValue.Equals(p.SpecsValue, StringComparison.InvariantCultureIgnoreCase) && SpecsName.Equals(p.SpecsName, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    [Serializable]
    public class ProductSpecItemRoot
    {
        public List<ProductSpecItem> selectedTotalSpecs { get; set; }
    }
}