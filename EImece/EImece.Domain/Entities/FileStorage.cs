using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class FileStorage : BaseEntity
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string MimeType { get; set; }
        public int FileSize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Type { get; set; }
    }
}
