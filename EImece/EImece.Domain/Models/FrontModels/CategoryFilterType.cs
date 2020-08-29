using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class CategoryFilterType
    {
        public List<CategoryFilter> CategoryFilters = new List<CategoryFilter>();
        public FilterTypeName FilterTypeName { get; set; }
        public int Position { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
    }

    public class FilterTypeName
    {
        public FilterType FilterType { get; set; }
        public string Text { get; set; }
    }

    public enum FilterType
    {
        Brand = 1,
        Price = 2,
        Rating = 3
    }
}