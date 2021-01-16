using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using GenericRepository;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Models.FrontModels
{
    public class SimiliarProductTagsViewModel : ItemListing
    {
        public String TagId { get; set; }
        public Tag Tag { get; set; }
        public PaginatedList<ProductTag> ProductTags { get; set; }
        public PaginatedList<StoryTag> StoryTags { get; set; }

        public string ProductsListPageUrl(SortingType sorting, IPaginatedModelList paginatedModelList)
        {
            var routeValues = ProductCategoryViewModel.GetRouteValueDictionary(paginatedModelList);
            var requestContext = HttpContext.Current.Request.RequestContext;
            var sortingInt = (int)sorting;
            routeValues.Remove("sorting");
            routeValues.Add("sorting", sortingInt);
            routeValues.Remove("RouteId");
            routeValues.Add("RouteId", TagId);
            var urlHelp = new UrlHelper(requestContext);
            if (string.IsNullOrEmpty(paginatedModelList.Filter))
            {
                routeValues.Remove("filtreler");
            }
            return urlHelp.Action("tag", "Products", routeValues);
        }
    }
}