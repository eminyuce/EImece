using EImece.Domain.GenericRepository;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using System.Net;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductsSearchViewModel : ItemListing
    {
        public string Search { get; set; }
        public PaginatedList<StorefrontProductCardDto> Products { get; set; }

        public StorefrontMenuDto ProductMenu { get; set; }
        public StorefrontMenuDto MainPageMenu { get; set; }

        public string ProductsListPageUrl(SortingType sorting, IPaginatedModelList paginatedModelList)
        {
            var sortingInt = (int)sorting;
            var search = !string.IsNullOrEmpty(Search) ? Search : (paginatedModelList?.Search ?? "");
            var filter = paginatedModelList?.Filter;

            var url = $"/products/searchproducts?sorting={sortingInt}";
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