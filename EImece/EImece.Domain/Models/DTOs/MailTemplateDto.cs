using System;

namespace EImece.Domain.Models.DTOs
{
    public class MailTemplateDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from MailTemplate
        public String Subject { get; set; }
        public String Body { get; set; }
        public String UpdateUserId { get; set; }
        public String AddUserId { get; set; }
        public bool TrackWithBitly { get; set; }
        public bool TrackWithMlnk { get; set; }
    }
}
