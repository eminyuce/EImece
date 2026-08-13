using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.AdminModels
{
    public class MailTemplateModelUsage
    {
        public List<string> PropertyPaths { get; set; }

        public Dictionary<string, List<string>> CollectionItemPaths { get; set; }

        public MailTemplateModelUsage()
        {
            PropertyPaths = new List<string>();
            CollectionItemPaths = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
