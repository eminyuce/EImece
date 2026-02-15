using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductsSearchResult
    {
        public RecordsStats Stats { get; set; }
        private List<Product> _products = new List<Product>();
        private List<ProductDto> _productDtos = new List<ProductDto>();

        public List<Product> Products
        {
            get { return _products; }
            set { _products = value; }
        }

        public List<ProductDto> ProductDtos
        {
            get { return _productDtos; }
            set { _productDtos = value; }
        }

        private List<ProductCategory> _productCategories = new List<ProductCategory>();
        private List<ProductCategoryDto> _productCategoryDtos = new List<ProductCategoryDto>();

        public List<ProductCategory> ProductCategories
        {
            get { return _productCategories; }
            set { _productCategories = value; }
        }

        public List<ProductCategoryDto> ProductCategoryDtos
        {
            get { return _productCategoryDtos; }
            set { _productCategoryDtos = value; }
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