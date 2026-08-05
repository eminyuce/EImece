using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EImece.Web.Models;

public sealed class AdminEditViewModel
{
    public string Title { get; set; } = "Düzenle";
    public string ControllerName { get; set; } = string.Empty;
    /// <summary>Controls optional fields in EntityEdit: default, product, brand, coupon, menu, faq, story, tag, template, setting.</summary>
    public string EditProfile { get; set; } = "default";
    public int Id { get; set; }

    [Required]
    [StringLength(500)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public int Position { get; set; }

    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? Code { get; set; }
    public string? Link { get; set; }
    public string? MenuLink { get; set; }
    public decimal Price { get; set; }
    public decimal? Discount { get; set; }
    public string? ProductCode { get; set; }
    public int? ProductCategoryId { get; set; }
    public int? BrandId { get; set; }
    public int? TagCategoryId { get; set; }
    public int? StoryCategoryId { get; set; }
    public int ParentId { get; set; }
    public bool MainPage { get; set; }
    public bool IsCampaign { get; set; }
    public int? MainImageId { get; set; }
    public bool LinkIsActive { get; set; }
    public string? Answer { get; set; }
    public string? SettingKey { get; set; }
    public string? SettingValue { get; set; }
    public string? TemplateXml { get; set; }
    public int DiscountPercentage { get; set; }
    public int CouponDiscountAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public IEnumerable<SelectListItem> ProductCategories { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Brands { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> TagCategories { get; set; } = Array.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> StoryCategories { get; set; } = Array.Empty<SelectListItem>();
}
