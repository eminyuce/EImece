using Resources;
using System;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.DTOs
{
    public class ProductCommentDto
    {
        // from BaseEntity
        public int Id { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Name))]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from ProductComment
        public int ProductId { get; set; }
        public string UserId { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Review))]
        public string Review { get; set; }

        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.NotValidEmailAddress))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        public string Email { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.SubjectLabel))]
        public string Subject { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Rating))]
        public int Rating { get; set; }

        public string SeoUrl { get; set; }
    }
}
