using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.Enums
{
    public enum ProductState
    {
        [Display(Name = "NONE")]
        // English: NONE
        // Turkish: YOK
        NONE = 0,

        [Display(Name = "Product In Stock")]
        // English: Product In Stock
        // Turkish: Ürün Stokta
        ProductInStock = 1,

        [Display(Name = "Product Out of Stock")]
        // English: Product Out of Stock
        // Turkish: Ürün Stokta Yok
        ProductOutOfStock = 2,

        [Display(Name = "Pre-Order Available")]
        // English: Pre-Order Available
        // Turkish: Ön Sipariş Mevcut
        PreOrder = 3,

        [Display(Name = "Discontinued")]
        // English: Discontinued
        // Turkish: Üretimden Kaldırıldı
        Discontinued = 4,

        [Display(Name = "Backorder Available")]
        // English: Backorder Available
        // Turkish: Geri Sipariş Mevcut
        Backorder = 5,

        [Display(Name = "Coming Soon")]
        // English: Coming Soon
        // Turkish: Yakında Geliyor
        ComingSoon = 6,

        [Display(Name = "Limited Stock Available")]
        // English: Limited Stock Available
        // Turkish: Sınırlı Stok Mevcut
        LimitedStock = 7,

        [Display(Name = "Reserved for Customers")]
        // English: Reserved for Customers
        // Turkish: Müşteriler İçin Ayrılmış
        Reserved = 8,

        [Display(Name = "Awaiting Restock")]
        // English: Awaiting Restock
        // Turkish: Yeniden Stok Bekleniyor
        AwaitingRestock = 9,

        [Display(Name = "Not for Sale")]
        // English: Not for Sale
        // Turkish: Satılık Değil
        NotForSale = 10
    }
}