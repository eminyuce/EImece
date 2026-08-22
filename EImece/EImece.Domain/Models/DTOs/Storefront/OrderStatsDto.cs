namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Aggregated order statistics for account headers (COUNT + SUM of PaidPrice).
    /// Replaces materializing full order lists just to compute Count/Sum.
    /// </summary>
    public class OrderStatsDto
    {
        public int TotalOrderCount { get; set; }
        public decimal TotalPaid { get; set; }
    }
}
