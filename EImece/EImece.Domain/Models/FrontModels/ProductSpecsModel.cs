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