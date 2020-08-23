using EImece.Domain.Models.Enums;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Models.FrontModels
{
    public class ItemListing
    {
        public int RouteId { set; get; }
        public int Page { get; set; }
        public int RecordPerPage { get; set; }
        public string Filter { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public SortingType Sorting { get; set; }

        public List<string> SelectedFilters
        {
            get
            {
                return Filter.Split("-".ToCharArray()).Select(r => r).ToList();
            }
        }
    }
}