using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductSpecsModel
    {
        public string groupName { get; set; }
        public string specsName { get; set; }
        public string value { get; set; }
        public string unit { get; set; }
        public string values { get; set; }

        public ProductSpecsModel(string specsName, string value, string unit, string values)
            : this(specsName, value, unit, values, null)
        {
        }

        public ProductSpecsModel(string specsName, string value, string unit, string values, string groupName)
        {
            this.specsName = specsName;
            this.value = value;
            this.unit = unit;
            this.values = values;
            this.groupName = groupName ?? string.Empty;
        }
    }

    public class ProductSpecsGroupModel
    {
        public string Name { get; set; }
        public IList<ProductSpecsModel> Items { get; set; }

        public ProductSpecsGroupModel(string name, IList<ProductSpecsModel> items)
        {
            Name = name ?? string.Empty;
            Items = items ?? new List<ProductSpecsModel>();
        }
    }
}
