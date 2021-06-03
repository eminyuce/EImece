using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Models.FrontModels.ShoppingCart
{
    [Serializable]
    public class ShoppingCartProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public string ProductCode { get; set; }
        public string CategoryName { get; set; }
        public string CroppedImageUrl { get; set; }
        private string _detailPageUrl { get; set; }
        public List<ProductSpecItem> ProductSpecItems { set; get; }

        public string DetailPageUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_detailPageUrl))
                {
                    var requestContext = HttpContext.Current.Request.RequestContext;
                    return new UrlHelper(requestContext).Action("Detail", "Products", new { id = Id });
                }
                return _detailPageUrl;
            }
            set
            {
                _detailPageUrl = value;
            }
        }

        public ShoppingCartProduct()
        {
            ProductSpecItems = new List<ProductSpecItem>();
        }

        public ShoppingCartProduct(Product product, List<ProductSpecItem> ProductSpecItems) : this()
        {
            this.Id = product.Id;
            this.Name = product.NameLong;
            this.Price = product.PriceWithDiscount;
            this.ProductCode = product.ProductCode;
            this.CategoryName = product.ProductCategory.Name;
            this.DetailPageUrl = product.DetailPageRelativeUrl;
            if (product.MainImageId.HasValue && product.ImageState)
            {
                this.CroppedImageUrl = product.GetCroppedImageUrl(product.MainImageId.Value, 200, 0);
            }
            this.ProductSpecItems.AddRange(ProductSpecItems);
        }
    }
}