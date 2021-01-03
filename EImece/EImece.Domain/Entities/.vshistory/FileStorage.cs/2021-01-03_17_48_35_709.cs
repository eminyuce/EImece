using EImece.Domain.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
        public bool IsFileExist { get; set; }

        public ICollection<FileStorageTag> FileStorageTags { get; set; }

        [NotMapped]
        public string DetailPageAbsoluteUrl
        {
            get
            {
                return FilesHelper.GetFileStorageSrcPath(this);
            }
        }
    }
}