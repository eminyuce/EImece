using System;

namespace EImece.Domain.Models.DTOs
{
    public class ProductSpecificationDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from ProductSpecification
        public string GroupName { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
        public int ProductId { get; set; }
    }
}
