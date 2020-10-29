using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

        public CategoryFilterType Parent { get; set; }

        public string RemoveSelectedFilter(List<CategoryFilter> SelectedFilterTypes)
        {
            if (SelectedFilterTypes.IsEmpty())
                return "";

            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            var filters = SelectedFilterTypes.Where(r => !r.CategoryFilterId.Equals(this.CategoryFilterId, StringComparison.InvariantCultureIgnoreCase)).ToList();
            return urlHelper.Action("Category", "ProductCategories", new { filtreler = string.Join("-", filters.Select(r=>r.CategoryFilterId)) });
        }

        public override bool Equals(object obj)
        {
            var item = obj as CategoryFilter;

            if (item == null)
            {
                return false;
            }

            return this.CategoryFilterId.Equals(item.CategoryFilterId,StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return name;
        }
    }
}