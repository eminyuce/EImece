using GenericRepository;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public String ProductSpecItems { set; get; }

        [NotMapped]
        public double Price
        {
            get
            {
                return TotalPrice / Quantity;
            }
        }

        public Product Product { get; set; }

        public OrderProduct()
        {
        }
    }
}