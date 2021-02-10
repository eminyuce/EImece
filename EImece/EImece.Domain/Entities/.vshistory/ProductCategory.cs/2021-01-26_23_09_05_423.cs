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
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ProductCategoryParentId))]
        public int ParentId { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MainPage))]
        public Boolean MainPage { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ShortDescription))]
        public string ShortDescription { get; set; }

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
                return new UrlHelper(requestContext).Action("Category", "ProductCategories", new { id = this.GetSeoUrl() });
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
        public string CreateChildDataContent
        {
            get
            {
                if (MainImage != null)
                {
                    var mainImageUrl = MainImage.GetCroppedImageUrl(
                  MainImage.Id,
                  300, 0, false);
                    var result = string.Format("<img src='{0}' class='d-block mt-n1' alt='{1}'><div class='text-center font-size-sm font-weight-semibold mt-n0 pb-0'>{1}</div>", mainImageUrl,
                        this.Name);
                    return HttpUtility.HtmlEncode(result);
                }
                else
                {
                    return "";
                }
            }
        }
    }
}