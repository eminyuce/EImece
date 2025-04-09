using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.Enums
{
    public enum EImeceOrderStatus
    {
        [Description("Yeni Sipariş")]
        [Display(Name = "NewlyOrder")]
        NewlyOrder = 1,

        [Description("Ödeme Bekleniyor")]
        [Display(Name = "PaymentPending")]
        PaymentPending = 2,

        [Description("Hazırlanmakta Olan Sipariş")]
        [Display(Name = "InProgress")]
        InProgress = 3,

        [Description("Kargoya Verildi")]
        [Display(Name = "Shipped")]
        Shipped = 4,

        [Description("Teslim Edildi")]
        [Display(Name = "Delivered")]
        Delivered = 5,

        [Description("İptal Edilmiş Sipariş")]
        [Display(Name = "Cancelled")]
        Cancelled = 6,

        [Description("Ürün iade edildi, para iadesi bekleniyor")]
        [Display(Name = "İade Edildi")]
        Returned = 7,

        [Description("Ürün iade edildi ve para iadesi yapıldı")]
        [Display(Name = "İade Tamamlandı")]
        Refunded = 8
    }
}