using EImece.Domain.Helpers;

namespace EImece.Domain.Models.FrontModels
{
    public class CategoryFilter
    {
        public string CategoryFilterId { get; set; }
        public int count { get; set; }
        public string name { get; set; }
        public int rating { get; set; }
        public int minPrice { get; set; }
        public int maxPrice { get; set; }
        public int ItemId { get { return CategoryFilterId.Substring(1).ToInt(); } }
    }
}