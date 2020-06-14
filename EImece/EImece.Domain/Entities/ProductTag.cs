using GenericRepository;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    public class ProductTag : IEntity<int>
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Tag")]
        public int TagId { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public Tag Tag { get; set; }
        public Product Product { get; set; }
    }
}