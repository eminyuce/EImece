using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels.ShoppingCart
{
    public class ShoppingCartProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }

        public string ProductCode { get; set; }
        public string CategoryName { get; set; }
        public string CroppedImageUrl { get; set; }

        public ShoppingCartProduct()
        {

        }
        public ShoppingCartProduct(Product product)
        {
            this.Id = product.Id;
            this.Name = product.Name;
            this.Price = product.Price;
            this.ProductCode = product.ProductCode;
            this.CategoryName = product.ProductCategory.Name;
            if (product.MainImageId.HasValue && product.ImageState)
            {
                this.CroppedImageUrl = product.GetCroppedImageUrl(product.MainImageId.Value, 80, 80);
            }
        }
    }
}
