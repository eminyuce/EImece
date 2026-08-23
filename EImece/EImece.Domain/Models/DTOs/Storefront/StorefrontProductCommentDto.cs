using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using System;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Projected approved product comment for storefront display.
    /// </summary>
    public class StorefrontProductCommentDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public string Review
        {
            get => Comment;
            set => Comment = value;
        }
        public string Email { get; set; }
        public string Subject { get; set; }
        public int Rating { get; set; }
        public string SeoUrl { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }

        public static StorefrontProductCommentDto FromEntity(ProductComment pc)
        {
            if (pc == null) return null;
            return new StorefrontProductCommentDto
            {
                Id = pc.Id,
                ProductId = pc.ProductId,
                UserId = pc.UserId,
                Name = pc.Name,
                Comment = pc.Review,
                Email = pc.Email,
                Subject = pc.Subject,
                Rating = pc.Rating,
                SeoUrl = pc.GetSeoUrl(),
                Position = pc.Position,
                Lang = pc.Lang,
                CreatedDate = pc.CreatedDate,
                UpdatedDate = pc.UpdatedDate,
                IsActive = pc.IsActive
            };
        }
    }
}
