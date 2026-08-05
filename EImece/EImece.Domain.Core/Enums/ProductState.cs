namespace EImece.Domain.Core.Enums;

public enum ProductState
{
    NONE = 0,
    ProductInStock = 1,
    ProductOutOfStock = 2,
    PreOrder = 3,
    Discontinued = 4,
    Backorder = 5,
    ComingSoon = 6,
    LimitedStock = 7,
    Reserved = 8,
    AwaitingRestock = 9,
    NotForSale = 10
}

public static class ProductStateLabels
{
    public static string ToTurkish(ProductState state) => state switch
    {
        ProductState.NONE => "YOK",
        ProductState.ProductInStock => "Ürün stokta",
        ProductState.ProductOutOfStock => "Ürün Stokta Yok",
        ProductState.PreOrder => "Ön Sipariş Mevcut",
        ProductState.Discontinued => "Üretimden Kaldırıldı",
        ProductState.Backorder => "Stok dışı sipariş mevcut",
        ProductState.ComingSoon => "Yakında Geliyor",
        ProductState.LimitedStock => "Sınırlı Stok Mevcut",
        ProductState.Reserved => "Müşteriler İçin Ayrılmış",
        ProductState.AwaitingRestock => "Yeniden Stok Bekleniyor",
        ProductState.NotForSale => "Satışa kapalı",
        _ => state.ToString()
    };

    public static ProductState Parse(string? state)
        => Enum.TryParse<ProductState>(state, true, out var result) ? result : ProductState.NONE;
}
