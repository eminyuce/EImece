using EImece.Domain.GenericRepository;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using System.Web;
using System.Web.Mvc;

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