using EImece.Domain.Entities;
using GenericRepository;
using System;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductsSearchViewModel
    {
        public String Search { get; set; }
        public PaginatedList<Product> Products { get; set; }

        public Menu ProductMenu { get; set; }
        public Menu MainPageMenu { get; set; }

    }
}
