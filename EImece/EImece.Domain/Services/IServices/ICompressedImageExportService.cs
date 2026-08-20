using EImece.Domain.Models.AdminModels;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface ICompressedImageExportService
    {
        Task<ImageExportPackageResult> ExportCompressedImagesAsync(
            string mediaImagesDirectory = null,
            long jpegQuality = 70L,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
