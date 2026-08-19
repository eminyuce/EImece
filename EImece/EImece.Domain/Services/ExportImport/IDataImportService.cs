using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.ExportImport
{
    public enum ImportMode
    {
        Insert = 1,
        Upsert = 2
    }

    public class DataImportRequest
    {
        public bool DryRun { get; set; } = true;
        public ImportMode Mode { get; set; } = ImportMode.Upsert;
        public HashSet<string> IncludedEntities { get; set; }
        public string ImportedBy { get; set; } = "System";
    }

    public class EntityImportValidationResult
    {
        public string Entity { get; set; }
        public int RecordCount { get; set; }
        public int ExistingCount { get; set; }
        public int NewCount { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class DataImportValidationResult
    {
        public bool IsValid { get; set; }
        public string FormatVersion { get; set; }
        public string Application { get; set; }
        public Dictionary<string, EntityImportValidationResult> EntityResults { get; set; } = new Dictionary<string, EntityImportValidationResult>();
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class DataImportResult
    {
        public bool Success { get; set; }
        public bool IsDryRun { get; set; }
        public int TotalRecordsProcessed { get; set; }
        public int TotalInserted { get; set; }
        public int TotalUpdated { get; set; }
        public int TotalSkipped { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public interface IDataImportService
    {
        Task<DataImportValidationResult> ValidateExportPackageAsync(Stream inputStream, CancellationToken cancellationToken = default(CancellationToken));
        Task<DataImportResult> ImportDataAsync(DataImportRequest request, Stream inputStream, CancellationToken cancellationToken = default(CancellationToken));
    }
}
