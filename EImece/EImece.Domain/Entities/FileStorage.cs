using EImece.Domain.Helpers;
using System;
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

        // Needed for Admin panel — admin file manager preview resolves thumbnail + original src paths via FilesHelper.
        [NotMapped]
        public Tuple<string, string> FileStorageSrcPaths
        {
            get
            {
                return FilesHelper.GetFileStorageSrcPath(this);
            }
        }
    }
}