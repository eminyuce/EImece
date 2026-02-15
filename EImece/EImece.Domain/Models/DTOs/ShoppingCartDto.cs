using System;

namespace EImece.Domain.Models.DTOs
{
    public class ShoppingCartDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from ShoppingCart
        public string OrderGuid { get; set; }
        public string ShoppingCartJson { get; set; }
        public string UserId { get; set; }
    }
}
