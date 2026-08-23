using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    public class ProductCategory : BaseContent
    {
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ProductCategoryParentId))]
        public int ParentId { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.MainPage))]
        public Boolean MainPage { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ShortDescription))]
        public string ShortDescription { get; set; }

        public ICollection<Product> Products { get; set; }

        [ForeignKey("Template")]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.TemplateId))]
        public int? TemplateId { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ProductCategoryDiscountPercantage))]
        public double? DiscountPercantage { get; set; }

        public string ProductCategoryListPageUrl(SortingType sorting, IPaginatedModelList paginatedModelList)
        {
            var routeValues = ProductCategoryViewModel.GetRouteValueDictionary(paginatedModelList);
            var requestContext = HttpContext.Current.Request.RequestContext;
            var sortingInt = (int)sorting;
            routeValues.Remove("sorting");
            routeValues.Add("sorting", sortingInt);
            var urlHelp = new UrlHelper(requestContext);
            if (string.IsNullOrEmpty(paginatedModelList.Filter))
            {
                routeValues.Remove("filtreler");
            }
            return urlHelp.Action("Category", "ProductCategories", routeValues);
        }

        public Template Template { get; set; }

        [NotMapped]
        public List<ProductCategory> Childrens { get; set; }

        [NotMapped]
        public ProductCategory Parent { get; set; }

        [NotMapped]
        public string ProductCategoryLink
        {
            get
            {
                var requestContext = HttpContext.Current.Request.RequestContext;
                var urlHelp = new UrlHelper(requestContext);
                return urlHelp.Action("Category", "ProductCategories", new { id = this.GetSeoUrl() });
            }
        }

        [NotMapped]
        public string DetailPageUrl
        {
            get
            {
                return this.GetDetailPageUrl("Category", "productCategories");
            }
        }

        [NotMapped]
        public string CreateChildDataContent
        {
            get
            {
                var dataContent = "";
                if (Childrens != null && Childrens.Count > 0)
                {
                    dataContent = "<ul>";
                    foreach (var category in Childrens)
                    {
                        dataContent = dataContent + String.Format("<li><a href='{0}'>{1}</a></li>", category.DetailPageUrl, category.Name);
                    }
                    dataContent = dataContent + "</ul>";
                }
                return dataContent;
            }
        }
    }
}