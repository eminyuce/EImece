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
    private string SpecsName { get; set; }
        private string SpecsValue { get; set; }
    }
    [Serializable]
    public class ProductSpecItemRoot
    {
        public List<ProductSpecItem> selectedTotalSpecs { get; set; }
    }

}
