using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs
{
    public class FileStorageDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from FileStorage
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string MimeType { get; set; }
        public int FileSize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Type { get; set; }
        public bool IsFileExist { get; set; }
    }
}
