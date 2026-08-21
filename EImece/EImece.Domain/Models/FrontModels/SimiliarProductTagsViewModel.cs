using EImece.Domain.GenericRepository;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Models.FrontModels
{
    public class SimiliarProductTagsViewModel : ItemListing
    {
        public string TagId { get; set; }
        public StorefrontTagDto Tag { get; set; }
        public PaginatedList<StorefrontProductCardDto> Products { get; set; }
        public PaginatedList<StorefrontStoryCardDto> StoryTags { get; set; }

        public string ProductsListPageUrl(SortingType sorting, IPaginatedModelList paginatedModelList)
        {
            var routeValues = ProductCategoryViewModel.GetRouteValueDictionary(paginatedModelList);
            var requestContext = HttpContext.Current.Request.RequestContext;
            var sortingInt = (int)sorting;
            routeValues.Remove("sorting");
            routeValues.Add("sorting", sortingInt);
            var urlHelp = new UrlHelper(requestContext);
            return urlHelp.Action("tag", "Products", routeValues);
        }
    }
}