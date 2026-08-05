using System.ComponentModel.DataAnnotations;

namespace EImece.Web.Models;

public sealed class DashboardViewModel
{
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
    public int CustomerCount { get; set; }
    public int SubscriberCount { get; set; }
    public IReadOnlyList<RecentOrderRow> RecentOrders { get; set; } = Array.Empty<RecentOrderRow>();
}

public sealed class RecentOrderRow
{
    public int Id { get; set; }
    public string? OrderNumber { get; set; }
    public string? Name { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PaidPrice { get; set; }
    public DateTime CreatedDate { get; set; }
}

public sealed class MetricsViewModel
{
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
    public int CustomerCount { get; set; }
    public int SubscriberCount { get; set; }
}

public sealed class MediaListViewModel
{
    public IReadOnlyList<MediaFileRow> Files { get; set; } = Array.Empty<MediaFileRow>();
    public int ContentId { get; set; }
    public string? Mod { get; set; }
    public string? ImageType { get; set; }
}

public sealed class MediaFileRow
{
    public int Id { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public string? MimeType { get; set; }
    public int FileSize { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public sealed class ImportDataIndexViewModel
{
    public IReadOnlyList<string> AppDataFiles { get; set; } = Array.Empty<string>();
    public string AppDataPath { get; set; } = string.Empty;
}

public sealed class ImportPreviewViewModel
{
    public string FileName { get; set; } = string.Empty;
    public System.Data.DataTable? Preview { get; set; }
}

public sealed class AdminSettingsViewModel
{
    public bool BypassAdminAuth { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string ApplicationLanguages { get; set; } = string.Empty;
    public int MainLanguage { get; set; }
    public IReadOnlyList<SettingRow> KeySettings { get; set; } = Array.Empty<SettingRow>();
}

public sealed class SystemSettingsViewModel
{
    public string SiteStatus { get; set; } = string.Empty;
    public bool IsSiteUnderConstruction { get; set; }
    public int MainLanguage { get; set; }
    public bool BypassAdminAuth { get; set; }
    public int DatabaseCommandTimeoutSeconds { get; set; }
}

public sealed class SettingRow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SettingKey { get; set; }
    public string? SettingValue { get; set; }
    public bool IsActive { get; set; }
}

public sealed class AppLogsViewModel
{
    public IReadOnlyList<LogFileRow> LogFiles { get; set; } = Array.Empty<LogFileRow>();
    public string LogDirectory { get; set; } = string.Empty;
    public string? SelectedLogContent { get; set; }
    public string? SelectedLogName { get; set; }
}

public sealed class LogFileRow
{
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime LastWriteUtc { get; set; }
}

public sealed class WebSiteLogoViewModel
{
    public int Id { get; set; }
    public string? CurrentLogoUrl { get; set; }
    public string? SettingValue { get; set; }
}

public sealed class AdminUserEditViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public sealed class AdminChangePasswordViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class AdminUserRolesViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool IsCustomer { get; set; }
}

public sealed class OrderDetailsViewModel
{
    public int Id { get; set; }
    public string? OrderNumber { get; set; }
    public string? Name { get; set; }
    public int OrderStatus { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PaidPrice { get; set; }
    public string? Price { get; set; }
    public decimal CargoPrice { get; set; }
    public string? AdminOrderNote { get; set; }
    public string? OrderComments { get; set; }
    public string? Coupon { get; set; }
    public string? ShipmentCompanyName { get; set; }
    public string? ShipmentTrackingNumber { get; set; }
    public DateTime CreatedDate { get; set; }
    public IReadOnlyList<OrderLineRow> Lines { get; set; } = Array.Empty<OrderLineRow>();
}

public sealed class OrderLineRow
{
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }
}

public sealed class OrderUpdateViewModel
{
    public int Id { get; set; }
    public int OrderStatus { get; set; }

    [StringLength(2000)]
    public string? AdminOrderNote { get; set; }
}

public sealed class ProductCommentListViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public IReadOnlyList<ProductCommentRow> Comments { get; set; } = Array.Empty<ProductCommentRow>();
}

public sealed class ProductCommentRow
{
    public int Id { get; set; }
    public string? Subject { get; set; }
    public string? Review { get; set; }
    public string? Email { get; set; }
    public int Rating { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public sealed class GridDeleteRequest
{
    public List<string> Values { get; set; } = new();
}

public sealed class ProductGridOrderingRequest
{
    public List<EImece.Domain.Core.Admin.OrderingItem> Values { get; set; } = new();
    public string? Checkbox { get; set; }
}

public sealed class ProductStateChangeRequest
{
    public List<string> Values { get; set; } = new();
    public int ProductStateSelection { get; set; }
}

public sealed class UpdatePriceRequest
{
    public decimal? PercentageOfIncreaseOrDecrease { get; set; }
    public int? ProductId { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public int? TagId { get; set; }
}

public sealed class ProductIdRequest
{
    public int ProductId { get; set; }
}

public sealed class ProductTagsRequest
{
    public int ProductId { get; set; }
    public int Language { get; set; }
}

public sealed class StoryTagsRequest
{
    public int StoryId { get; set; }
    public int Language { get; set; }
}

public sealed class SaveAdminOrderNoteRequest
{
    public int OrderId { get; set; }
    public string? AdminOrderNote { get; set; }
    public string? ShipmentCompanyName { get; set; }
    public string? ShipmentTrackingNumber { get; set; }
}

public sealed class ChangedOrderStatusRequest
{
    public int OrderId { get; set; }
    public string? OrderStatus { get; set; }
}

public sealed class DeleteMainImageRequest
{
    public int ContentId { get; set; }
    public int ImageId { get; set; }
    public string? ContentClass { get; set; }
}

public sealed class SearchAutoCompleteRequest
{
    public string? Term { get; set; }
    public string? Action { get; set; }
    public string? Controller { get; set; }
}
