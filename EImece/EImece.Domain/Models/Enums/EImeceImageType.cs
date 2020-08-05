using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.Enums
{
    public enum EImeceImageType
    {
        [Display(Name = "NONE")]
        NONE = 0,

        [Display(Name = "ProductGallery")]
        ProductGallery = 1,

        [Display(Name = "StoryGallery")]
        StoryGallery = 2,

        [Display(Name = "Images")]
        Images = 3,

        [Display(Name = "ProductMainImage")]
        ProductMainImage = 4,

        [Display(Name = "ProductCategoryMainImage")]
        ProductCategoryMainImage = 5,

        [Display(Name = "StoryCategoryMainImage")]
        StoryCategoryMainImage = 6,

        [Display(Name = "StoryMainImage")]
        StoryMainImage = 6,

        [Display(Name = "Carusels")]
        Carusels = 7,

        [Display(Name = "MenuMainImage")]
        MenuMainImage = 8,

        [Display(Name = "MainPageImages")]
        MainPageImages = 9,

        [Display(Name = "MenuGallery")]
        MenuGallery = 10,

        [Display(Name = "WebSiteLogo")]
        WebSiteLogo = 11,

        [Display(Name = "BrandMainImage")]
        BrandMainImage = 4,
    }
}