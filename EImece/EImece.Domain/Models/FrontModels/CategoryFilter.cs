using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

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

        public int ItemId
        { get { return CategoryFilterId.Substring(1).ToInt(); } }

        public CategoryFilterType Parent { get; set; }

        public string RemoveSelectedFilter(List<CategoryFilter> SelectedFilterTypes, IPaginatedModelList paginatedModelList)
        {
            if (SelectedFilterTypes.IsEmpty())
                return "";

            var filters = SelectedFilterTypes.Where(r => !r.CategoryFilterId.Equals(this.CategoryFilterId, StringComparison.InvariantCultureIgnoreCase)).ToList();

            var routeId = paginatedModelList?.RouteId ?? "";
            var remaining = string.Join("-", filters.Select(r => r.CategoryFilterId));

            var idPart = !string.IsNullOrEmpty(routeId) ? $"/{routeId}" : "";
            var url = $"/productcategories/category{idPart}";
            var queryParams = new List<string>();

            if (paginatedModelList?.Sorting.HasValue == true && paginatedModelList.Sorting.Value > 0)
            {
                queryParams.Add($"sorting={paginatedModelList.Sorting.Value}");
            }
            if (!string.IsNullOrEmpty(remaining))
            {
                queryParams.Add($"filtreler={WebUtility.UrlEncode(remaining)}");
            }
            if (!string.IsNullOrEmpty(paginatedModelList?.Search))
            {
                queryParams.Add($"search={WebUtility.UrlEncode(paginatedModelList.Search)}");
            }
            if (paginatedModelList?.MinPrice.HasValue == true && paginatedModelList.MinPrice.Value > 0)
            {
                queryParams.Add($"minPrice={paginatedModelList.MinPrice.Value}");
            }
            if (paginatedModelList?.MaxPrice.HasValue == true && paginatedModelList.MaxPrice.Value > 0)
            {
                queryParams.Add($"maxPrice={paginatedModelList.MaxPrice.Value}");
            }

            if (queryParams.Count > 0)
            {
                url += "?" + string.Join("&", queryParams);
            }

            return url;
        }

        public override bool Equals(object obj)
        {
            var item = obj as CategoryFilter;

            if (item == null)
            {
                return false;
            }

            return this.CategoryFilterId.Equals(item.CategoryFilterId, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return this.CategoryFilterId.GetHashCode();
        }
    }
}