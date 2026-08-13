using System.Collections.Generic;

namespace EImece.Domain.Models.AdminModels
{
    public class SendMailTemplateTestRequest
    {
        public int Id { get; set; }

        public string RecipientEmail { get; set; }

        public string SubjectOverride { get; set; }

        /// <summary>
        /// Optional unsaved editor body. When empty, the stored template body is used.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Optional unsaved editor subject. When empty, the stored template subject is used.
        /// </summary>
        public string Subject { get; set; }

        public Dictionary<string, string> ModelData { get; set; }
    }
}
