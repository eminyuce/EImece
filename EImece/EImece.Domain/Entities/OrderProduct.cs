using EImece.Domain.GenericRepository;
using EImece.Domain.Helpers;
using EImece.Domain.Models.FrontModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace EImece.Domain.Entities
{
    public class OrderProduct : IEntity<int>
    {
        // Entity annotions
        //[DataType(DataType.Text)]
        //[StringLength(100, ErrorMessage = "TestColumnName cannot be longer than 100 characters.")]
        //[Display(Name ="TestColumnName")]
        //[Required(ErrorMessage ="TestColumnName")]
        //[AllowHtml]
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public int? ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ProductSalePrice { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string CategoryName { get; set; }
        public string ProductSpecItems { set; get; }
        public string ProductImageUrl { get; set; }

        // Helper aliases for templates / serialization
        [NotMapped]
        public string Name
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ProductName) ? ProductName : (Product != null ? Product.Name : string.Empty);
            }
        }

        [NotMapped]
        public int Count
        {
            get
            {
                return Quantity;
            }
        }

        // Needed for Admin panel — admin order detail shows unit price (TotalPrice / Quantity) without an extra column.
        [NotMapped]
        public decimal Price
        {
            get
            {
                if (Quantity == 0)
                {
                    return ProductSalePrice;
                }
                return TotalPrice / Quantity;
            }
        }

        // Needed for Admin panel — admin order detail deserializes the spec JSON for display in the order lines grid.
        [NotMapped]
        public List<ProductSpecItem> ProductSpecObjItems
        {
            get
            {
                return JsonConvert.DeserializeObject<List<ProductSpecItem>>(ProductSpecItems.ToStr());
            }
        }

        // Needed for Admin panel — admin order detail highlights the first (color) spec item for quick preview.
        [NotMapped]
        public ProductSpecItem ProductSpecColorItem
        {
            get
            {
                return ProductSpecObjItems.FirstOrDefault();
            }
        }

        public virtual Product Product { get; set; }

        public OrderProduct()
        {
        }
    }
}