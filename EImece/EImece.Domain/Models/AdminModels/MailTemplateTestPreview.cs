using System.Collections.Generic;

namespace EImece.Domain.Models.AdminModels
{
    public class MailTemplateTestPreview
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Subject { get; set; }

        public List<MailTemplateModelProperty> Properties { get; set; }
    }
}
