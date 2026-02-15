using System;

namespace EImece.Domain.Models.DTOs
{
    public class ProductFileDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from ProductFile
        public int FileStorageId { get; set; }
        public int ProductId { get; set; }
        
        // Computed properties for frontend
        public string MainImageUrl { get; set; }
    }
}
