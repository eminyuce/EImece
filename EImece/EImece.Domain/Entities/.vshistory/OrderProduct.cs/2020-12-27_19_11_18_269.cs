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
        public double TotalPrice { get; set; }
        public string ProductSpecItems { set; get; }

        [NotMapped]
        public double Price
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
    }
}