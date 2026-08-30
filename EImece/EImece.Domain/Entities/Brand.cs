using EImece.Domain.Helpers.Extensions;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class Brand : BaseContent
    {
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.MainPage))]
        public Boolean MainPage { get; set; }

        public ICollection<Product> Products { get; set; }

        /// <summary>
        /// Category listing URL with this brand pre-selected (filtreler=b{id}).
        /// Falls back to product search by brand name when no category is available.
        /// </summary>
        public string GetProductsUrl(ProductCategory category)
        {
            if (Id <= 0 || string.IsNullOrWhiteSpace(Name))
            {
                return string.Empty;
            }

            if (category != null)
            {
                return $"/productcategories/category/{category.GetSeoUrl()}?filtreler=b{Id}";
            }

            return $"/products/searchproducts?search={WebUtility.UrlEncode(Name)}";
        }
    }
}