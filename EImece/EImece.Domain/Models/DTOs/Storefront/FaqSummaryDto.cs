namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Minimal FAQ for storefront accordion — only fields rendered in Faq.cshtml / SendMessageToSeller.
    /// Projection: SELECT Id, Question, Answer FROM Faqs WHERE Lang=@lang AND IsActive (3 cols).
    /// Omits Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, AddUserId, UpdateUserId.
    /// </summary>
    public class FaqSummaryDto
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
