using EImece.Domain.Helpers;
using EImece.Domain.Models.FrontModels;
using GenericRepository;
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
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ProductSalePrice { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string CategoryName { get; set; }
        public string ProductSpecItems { set; get; }
        
        [NotMapped]
        public decimal Price
        {
            get
            {
                return TotalPrice / Quantity;
            }
        }

        [NotMapped]
        public List<ProductSpecItem> ProductSpecObjItems
        {
            get
            {
                return JsonConvert.DeserializeObject<List<ProductSpecItem>>(ProductSpecItems.ToStr());
            }
        }

        [NotMapped]
        public ProductSpecItem ProductSpecColorItem
        {
            get
            {
                return ProductSpecObjItems.FirstOrDefault();
            }
        }

        public Product Product { get; set; }

        public OrderProduct()
        {
        }

        public override string ToString()
        {
            return $"{{{nameof(Id)}={Id.ToString()}, {nameof(OrderId)}={OrderId.ToString()}, {nameof(ProductId)}={ProductId.ToString()}, {nameof(Quantity)}={Quantity.ToString()}, {nameof(TotalPrice)}={TotalPrice.ToString()}, {nameof(ProductSalePrice)}={ProductSalePrice.ToString()}, {nameof(ProductName)}={ProductName}, {nameof(ProductCode)}={ProductCode}, {nameof(CategoryName)}={CategoryName}, {nameof(ProductSpecItems)}={ProductSpecItems}, {nameof(Price)}={Price.ToString()}, {nameof(ProductSpecObjItems)}={ProductSpecObjItems}, {nameof(ProductSpecColorItem)}={ProductSpecColorItem}, {nameof(Product)}={Product}}}";
        }
    }
}