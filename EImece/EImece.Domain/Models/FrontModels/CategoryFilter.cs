namespace EImece.Domain.Models.FrontModels
{
    public class CategoryFilter
    {
        public int CategoryFilterId { get; set; }
        public int count { get; set; }
        public string name { get; set; }
        public int minPrice { get; set; }
        public int maxPrice { get; set; }
    }
}