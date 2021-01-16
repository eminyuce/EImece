using EImece.Domain.Entities;
using GenericRepository;

namespace EImece.Domain.Models.FrontModels
{
    public class SimiliarProductTagsViewModel : ItemListing
    {
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
            routeValues.Remove("search");
            routeValues.Add("search", Search);
            var urlHelp = new UrlHelper(requestContext);
            if (string.IsNullOrEmpty(paginatedModelList.Filter))
            {
                routeValues.Remove("filtreler");
            }
            return urlHelp.Action("searchproducts", "Products", routeValues);
        }
    }
}