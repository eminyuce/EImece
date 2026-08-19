using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.AdminModels
{
    public class ImageExportPackageResult
    {
        public byte[] ZipBytes { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; } = "application/zip";
        public int TotalImageCount { get; set; }
        public long TotalOriginalSizeBytes { get; set; }
        public long TotalCompressedSizeBytes { get; set; }
    }

    public class ImageMetadataMapping
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int? FileStorageId { get; set; }
        public string MimeType { get; set; }
        public long OriginalSizeBytes { get; set; }
        public long CompressedSizeBytes { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public List<ImageRelatedRecordRef> RelatedRecords { get; set; } = new List<ImageRelatedRecordRef>();
    }

    public class ImageRelatedRecordRef
    {
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string RelationType { get; set; }
        public string RecordTitle { get; set; }
        public Dictionary<string, object> AdditionalReferenceData { get; set; } = new Dictionary<string, object>();
    }
}
