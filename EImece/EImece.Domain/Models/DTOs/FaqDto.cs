using System;

namespace EImece.Domain.Models.DTOs
{
    public class FaqDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from Faq
        public string Question { get; set; }
        public string Answer { get; set; }
        public string AddUserId { get; set; }
        public string UpdateUserId { get; set; }
    }
}
