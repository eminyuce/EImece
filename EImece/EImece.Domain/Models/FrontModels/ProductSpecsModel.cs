using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductSpecsModel
    {
        public string specsName { get; set; }
        public string value { get; set; }
        public string unit { get; set; }
        public string values { get; set; }

        public ProductSpecsModel(string specsName, string value, string unit, string values)
        {
            this.specsName = specsName;
            this.value = value;
            this.unit = unit;
            this.values = values;
        }
    }
}
