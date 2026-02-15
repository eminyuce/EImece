using System;

namespace EImece.Domain.Models.DTOs
{
    public class ShortUrlDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from ShortUrl
        public string UrlKey { get; set; }
        public string Url { get; set; }
        public int RequestCount { get; set; }
    }
}
