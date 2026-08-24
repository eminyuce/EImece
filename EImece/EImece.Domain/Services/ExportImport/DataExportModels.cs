using System;
using System.Collections.Generic;

namespace EImece.Domain.Services.ExportImport
{
    public class ExportManifest
    {
        public string Format { get; set; } = "application-data-export";
        public string FormatVersion { get; set; } = "1.0";
        public string Application { get; set; } = "EImece";
        public string DatabaseProvider { get; set; } = "SqlServer";
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public string Environment { get; set; }
        public Dictionary<string, ExportEntityManifestEntry> Entities { get; set; } = new Dictionary<string, ExportEntityManifestEntry>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<string>> ExcludedFields { get; set; } = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        public string Checksum { get; set; }
    }

    public class ExportEntityManifestEntry
    {
        public string File { get; set; }
        public int RecordCount { get; set; }
        public int SchemaVersion { get; set; } = 1;
    }

    public class ExportMetadata
    {
        public string ExportId { get; set; } = Guid.NewGuid().ToString("N");
        public string Application { get; set; } = "EImece";
        public string ApplicationVersion { get; set; } = "1.0";
        public string ExportedBy { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public long DurationMs { get; set; }
        public int TotalRecords { get; set; }
        public long TotalSizeCompressedBytes { get; set; }
        public List<string> IncludedEntities { get; set; } = new List<string>();
    }

    public class DataExportRequest
    {
        public int BatchSize { get; set; } = 500;
        public HashSet<string> IncludedEntities { get; set; }
        public string ExportedBy { get; set; } = "System";

        public bool ShouldExport(string entityName)
        {
            return IncludedEntities == null || IncludedEntities.Count == 0 || IncludedEntities.Contains(entityName);
        }
    }

    public class DataExportResult
    {
        public bool Success { get; set; }
        public int TotalRecords { get; set; }
        public long CompressedSizeBytes { get; set; }
        public ExportManifest Manifest { get; set; }
        public ExportMetadata Metadata { get; set; }
        public TimeSpan Duration { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class DataExportSummary
    {
        public string Application { get; set; }
        public string DatabaseProvider { get; set; }
        public Dictionary<string, int> EstimatedCounts { get; set; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        public int TotalEstimatedRecords { get; set; }
    }

    public class EntityExportContainer<T>
    {
        public string Entity { get; set; }
        public int SchemaVersion { get; set; } = 1;
        public int RecordCount { get; set; }
        public List<T> Records { get; set; } = new List<T>();
    }

    #region Export DTOs

    public class SettingExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class MailTemplateExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool TrackWithBitly { get; set; }
        public bool TrackWithMlnk { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class FaqExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class SubscriberExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class FileStorageExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string MimeType { get; set; }
        public int FileSize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Type { get; set; }
        public bool IsFileExist { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class FileStorageTagExportDto
    {
        public int Id { get; set; }
        public int FileStorageId { get; set; }
        public int TagId { get; set; }
    }

    public class TagCategoryExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class TagExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TagCategoryId { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class TemplateExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TemplateXml { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class ListExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsService { get; set; }
        public bool IsValues { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class ListItemExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ListId { get; set; }
        public string Value { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class ProductCategoryExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public int ParentId { get; set; }
        public int? TemplateId { get; set; }
        public int? MainImageId { get; set; }
        public bool MainPage { get; set; }
        public double? DiscountPercantage { get; set; }
        public bool ImageState { get; set; }
        public string MetaKeywords { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class BrandExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? MainImageId { get; set; }
        public bool MainPage { get; set; }
        public bool ImageState { get; set; }
        public string MetaKeywords { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class ProductExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameShort { get; set; }
        public string NameLong { get; set; }
        public int ProductCategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? MainImageId { get; set; }
        public string ProductCode { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public string State { get; set; }
        public bool MainPage { get; set; }
        public bool IsCampaign { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public string ProductColorOptions { get; set; }
        public string ProductSizeOptions { get; set; }
        public double Rating { get; set; }
        public bool ImageState { get; set; }
        public string MetaKeywords { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class ProductSpecificationExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProductId { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class ProductFileExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProductId { get; set; }
        public int FileStorageId { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class ProductTagExportDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int TagId { get; set; }
    }

    public class ProductCommentExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public string Review { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public int Rating { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class CouponExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int DiscountPercentage { get; set; }
        public int Discount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class StoryCategoryExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PageTheme { get; set; }
        public int? MainImageId { get; set; }
        public bool ImageState { get; set; }
        public string MetaKeywords { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class StoryExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StoryCategoryId { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string AuthorName { get; set; }
        public bool MainPage { get; set; }
        public bool IsFeaturedStory { get; set; }
        public int? MainImageId { get; set; }
        public bool ImageState { get; set; }
        public string MetaKeywords { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class StoryFileExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StoryId { get; set; }
        public int FileStorageId { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class StoryTagExportDto
    {
        public int Id { get; set; }
        public int StoryId { get; set; }
        public int TagId { get; set; }
    }

    public class MenuExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ParentId { get; set; }
        public string MenuLink { get; set; }
        public string Link { get; set; }
        public string PageTheme { get; set; }
        public bool LinkIsActive { get; set; }
        public bool MainPage { get; set; }
        public int? MainImageId { get; set; }
        public bool ImageState { get; set; }
        public string MetaKeywords { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class MenuFileExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MenuId { get; set; }
        public int FileStorageId { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class MainPageImageExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public int? MainImageId { get; set; }
        public bool ImageState { get; set; }
        public string MetaKeywords { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class CustomerExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string GsmNumber { get; set; }
        public string Email { get; set; }
        public string IdentityNumber { get; set; }
        public string UserId { get; set; }
        public bool IsPermissionGranted { get; set; }
        public int Gender { get; set; }
        public string Street { get; set; }
        public string Town { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string Description { get; set; }
        public string Company { get; set; }
        public int CustomerType { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class AddressExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int AddressType { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string Street { get; set; }
        public string District { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class OrderExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OrderNumber { get; set; }
        public string OrderGuid { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string UserId { get; set; }
        public int OrderType { get; set; }
        public int OrderStatus { get; set; }
        public string AdminOrderNote { get; set; }
        public string OrderComments { get; set; }
        public decimal CargoPrice { get; set; }
        public int ShippingAddressId { get; set; }
        public int BillingAddressId { get; set; }
        public string Coupon { get; set; }
        public string CouponDiscount { get; set; }
        public string Price { get; set; }
        public string PaidPrice { get; set; }
        public string Installment { get; set; }
        public string Currency { get; set; }
        public string PaymentStatus { get; set; }
        public string ShipmentTrackingNumber { get; set; }
        public string ShipmentCompanyName { get; set; }
        public string Locale { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class OrderProductExportDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ProductSalePrice { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string CategoryName { get; set; }
        public string ProductSpecItems { get; set; }
        public string ProductImageUrl { get; set; }
    }

    public class ShoppingCartExportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OrderGuid { get; set; }
        public string ShoppingCartJson { get; set; }
        public string UserId { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class UserExportDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool TwoFactorAuthenticatorEnabled { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class RoleExportDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    #endregion
}
