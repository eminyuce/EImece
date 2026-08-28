using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs
{
    public class ProductDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from BaseContent
        public string Description { get; set; }
        public bool ImageState { get; set; }
        public string MetaKeywords { get; set; }
        public int? MainImageId { get; set; }
        public int ImageHeight { get; set; }
        public int ImageWidth { get; set; }
        public string UpdateUserId { get; set; }
        public string AddUserId { get; set; }

        // from Product
        public string NameShort { get; set; }
        public string NameLong { get; set; }
        public int ProductCategoryId { get; set; }
        public int? BrandId { get; set; }
        public bool MainPage { get; set; }
        public string ShortDescription { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public string PriceStr { get; set; }
        public string DiscountStr { get; set; }
        public string ProductCode { get; set; }
        public string VideoUrl { get; set; }
        public bool IsCampaign { get; set; }
        public ProductState StateEnum
        {
            get => Enum.TryParse(State, true, out ProductState result) ? result : ProductState.NONE;
            set => State = value.ToString();
        }
        public string State { get; set; }
        public string ProductSizeOptions { get; set; }
        public double Rating { get; set; }
        public string DetailPageAbsoluteUrl { get; set; }
        public string DetailPageRelativeUrl { get; set; }
        public string BuyNowRelativeUrl { get; set; }
        public bool HasDiscount
        {
            get => (Discount.HasValue && Discount.Value > 0);
            set { }
        }
        public int DiscountPercentage
        {
            get
            {
                if (Price > 0 && Discount.HasValue && Discount.Value > 0)
                {
                    return (int)Math.Round((Discount.Value * 100) / Price, MidpointRounding.AwayFromZero);
                }
                return 0;
            }
            set { }
        }
        public string ModifiedId
        {
            get => EImece.Domain.Helpers.GeneralHelper.ModifyId(Id);
            set { }
        }
        public byte[] MainImageBytes { get; set; }
        public Tuple<string, string> MainImageSrc { get; set; }
        public string ProductNameStr
        {
            get
            {
                if (!string.IsNullOrEmpty(NameShort)) return NameShort;
                if (!string.IsNullOrEmpty(NameLong)) return NameLong;
                return Name;
            }
            set { }
        }
        public decimal PriceWithDiscount
        {
            get => HasDiscount ? Price - (Discount ?? 0) : Price;
            set { }
        }
        public bool IsBuyableState
        {
            get => ProductStateHelper.IsSuitableForSale(StateEnum, Price);
            set { }
        }
        public bool IsOnSale
        {
            get => ProductStateHelper.IsSuitableForSale(StateEnum, Price);
            set { }
        }
    }
}
