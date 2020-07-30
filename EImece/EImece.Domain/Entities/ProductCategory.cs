using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using Microsoft.Owin.Security.Provider;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    public class ProductCategory : BaseContent
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductCategoryParentId))]
        public int ParentId { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MainPage))]
        public Boolean MainPage { get; set; }

        public ICollection<Product> Products { get; set; }

        [ForeignKey("Template")]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.TemplateId))]
        public int? TemplateId { get; set; }

        [NotMapped]
        public List<ProductCategory> Childrens { get; set; }

        [NotMapped]
        public ProductCategory Parent { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductCategoryDiscountPercantage))]
        public double? DiscountPercantage { get; set; }

        [NotMapped]
        public string ProductCategoryLink  
        {
            get
             {
                var requestContext = HttpContext.Current.Request.RequestContext;
                return new UrlHelper(requestContext).Action("Category", "ProductCategories", new { id = this.GetSeoUrl()});
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

        public string ProductCategoryListPageUrl(SortingType sorting)
        {
            var requestContext = HttpContext.Current.Request.RequestContext;
            var sortingInt = (int)sorting;
            return new UrlHelper(requestContext).Action("Category", "ProductCategories", new { id = this.GetSeoUrl() , sorting= sortingInt });
        }

        public Template Template { get; set; }
    }
}