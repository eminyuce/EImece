using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using GenericRepository;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductsSearchViewModel : ItemListing
    {
        public string Search { get; set; }
        public PaginatedList<Product> Products { get; set; }

        public Menu ProductMenu { get; set; }
        public Menu MainPageMenu { get; set; }
        public List<Setting> ApplicationSettings { get; set; }

        private Setting GetSetting(string key)
        {
            if(ApplicationSettings == null)
            {
                return null;
            }
            return ApplicationSettings.FirstOrDefault(t => t.SettingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool IsProductPriceEnable
        {
            get
            {
                var item = GetSetting(Constants.IsProductPriceEnable);
                if(item == null)
                {
                    return false;
                }
                else
                {
                    return item.SettingValue.ToStr().Equals("true", StringComparison.InvariantCultureIgnoreCase);
                }
            }
        }
        public bool IsProductCommentSectionEnable
        {
            get
            {
                var item = GetSetting(Constants.IsProductCommentSectionEnable);
                if(item == null)
                {
                    return false;
                }
                else
                {
                    return item.SettingValue.ToStr().Equals("true", StringComparison.InvariantCultureIgnoreCase);
                }
            }
        }
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