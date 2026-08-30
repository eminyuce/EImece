using EImece.Domain.GenericRepository;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using System.Net;

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
            var sortingInt = (int)sorting;
            var routeId = !string.IsNullOrEmpty(paginatedModelList?.RouteId) ? paginatedModelList.RouteId : TagId;
            var search = paginatedModelList?.Search;
            var filter = paginatedModelList?.Filter;

            var idPart = !string.IsNullOrEmpty(routeId) ? $"/{routeId}" : "";
            var url = $"/products/tag{idPart}?sorting={sortingInt}";
            if (!string.IsNullOrEmpty(search))
            {
                url += $"&search={WebUtility.UrlEncode(search)}";
            }
            if (!string.IsNullOrEmpty(filter))
            {
                url += $"&filtreler={WebUtility.UrlEncode(filter)}";
            }
            return url;
        }
    }
}