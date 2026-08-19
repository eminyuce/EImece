using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.ExportImport
{
    public interface IDataExportService
    {
        Task<DataExportResult> ExportDataAsync(DataExportRequest request, Stream outputStream, CancellationToken cancellationToken = default(CancellationToken));
        Task<DataExportSummary> GetExportMetadataAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
