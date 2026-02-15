using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs
{
    public class OrderProductDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ProductSalePrice { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string CategoryName { get; set; }
        public string ProductSpecItems { get; set; }
        public decimal Price { get; set; }
        public List<ProductSpecItem> ProductSpecObjItems { get; set; }
        public ProductSpecItem ProductSpecColorItem { get; set; }
    }

    public class ProductSpecItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string ImageUrl { get; set; }
    }
}
