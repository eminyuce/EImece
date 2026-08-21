using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductsSearchResult
    {
        public RecordsStats Stats { get; set; }
        private List<StorefrontProductCardDto> _products = new List<StorefrontProductCardDto>();

        public List<StorefrontProductCardDto> Products
        {
            get { return _products; }
            set { _products = value; }
        }

        private List<StorefrontCategoryDto> _productCategories = new List<StorefrontCategoryDto>();

        public List<StorefrontCategoryDto> ProductCategories
        {
            get { return _productCategories; }
            set { _productCategories = value; }
        }

        public List<Filter> _filters = new List<Filter>();

        public List<Filter> Filters
        {
            get { return _filters; }
            set { _filters = value; }
        }

        public List<FilterGroup> FiltersGroups
        {
            get
            {
                var groups = new List<FilterGroup>();

                var groupNames = Filters.Select(i => i.FieldName).Distinct().ToList();
                foreach (var groupName in groupNames)
                {
                    var group = new FilterGroup(groupName);
                    group.Filters.AddRange(Filters.Where(r => r.FieldName.Equals(groupName)));
                    groups.Add(group);
                }
                return groups;
            }
        }

        public int PageSize { get; set; }
    }
}