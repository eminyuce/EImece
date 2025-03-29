namespace EImece.Domain.Models.AdminModels
{
    public class UpdatePriceRequest
    {
        public decimal? PercentageOfIncreaseOrDecrease { get; set; }
        public int? ProductId { get; set; } // Opsiyonel
        public int? CategoryId { get; set; } // Opsiyonel
        public int? BrandId { get; set; } // Opsiyonel
        public int? TagId { get; set; } // Opsiyonel
    }
}