using System;

namespace EImece.Domain.Models.DTOs
{
    public class StoryFileDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from StoryFile
        public int StoryId { get; set; }
        public int FileStorageId { get; set; }
    }
}
