using EImece.Domain.Entities;
using System;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Projected specification item for storefront product detail.
    /// </summary>
    public class StorefrontProductSpecificationDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
        public string GroupName { get; set; }
        public int Order { get; set; }
        public int Position
        {
            get => Order;
            set => Order = value;
        }
        public bool IsActive { get; set; }
        public int Lang { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public static StorefrontProductSpecificationDto FromEntity(ProductSpecification ps)
        {
            if (ps == null) return null;
            return new StorefrontProductSpecificationDto
            {
                Id = ps.Id,
                ProductId = ps.ProductId,
                Name = ps.Name,
                Value = ps.Value,
                Unit = ps.Unit,
                GroupName = ps.GroupName,
                Order = ps.Position,
                Position = ps.Position,
                IsActive = ps.IsActive,
                Lang = ps.Lang,
                CreatedDate = ps.CreatedDate,
                UpdatedDate = ps.UpdatedDate
            };
        }
    }
}
