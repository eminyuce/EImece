using EImece.Domain.Models.Enums;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class Coupon : BaseEntity
    {
        public Coupon()
        {
            CouponProducts = new HashSet<CouponProduct>();
            CouponCategories = new HashSet<CouponCategory>();
            CouponRedemptions = new HashSet<CouponRedemption>();
        }

        [Required(ErrorMessage = "Code is required")]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.CouponCode))]
        public string Code { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.DiscountPercentage))]
        public int DiscountPercentage { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Discount))]
        public int Discount { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.StartDate))]
        public DateTime StartDate { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.EndDate))]
        public DateTime EndDate { get; set; }

        // Needed for Admin panel — admin coupon form uses string-bound date inputs for jQuery datepicker.
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.StartDate))]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [NotMapped]
        public string StartDateStr { get; set; }

        // Needed for Admin panel — admin coupon form uses string-bound date inputs for jQuery datepicker.
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.EndDate))]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [NotMapped]
        public string EndDateStr { get; set; }

        // Per-customer coupon assignment (null = global)
        [Display(Name = "AssignedUserId")]
        [MaxLength(128)]
        public string AssignedUserId { get; set; }

        [Display(Name = "AssignedCustomerId")]
        public int? AssignedCustomerId { get; set; }

        [NotMapped]
        public string AssignedCustomerDisplay { get; set; }

        // Advanced coupon fields
        [Display(Name = "DiscountType")]
        public CouponDiscountType DiscountType { get; set; }

        [Display(Name = "MaxDiscountAmount")]
        public decimal? MaximumDiscountAmount { get; set; }

        [Display(Name = "GlobalUsageLimit")]
        public int? GlobalUsageLimit { get; set; }

        [Display(Name = "PerCustomerUsageLimit")]
        public int? PerCustomerUsageLimit { get; set; }

        [Display(Name = "MinimumOrderAmount")]
        public decimal? MinimumOrderAmount { get; set; }

        [Display(Name = "ExcludeSaleItems")]
        public bool ExcludeSaleItems { get; set; }

        [Display(Name = "IsFreeShipping")]
        public bool IsFreeShipping { get; set; }

        [Display(Name = "AllowStacking")]
        public bool AllowStacking { get; set; }

        [Display(Name = "RequireLogin")]
        public bool RequireLogin { get; set; }

        [Display(Name = "IsFirstOrderOnly")]
        public bool IsFirstOrderOnly { get; set; }

        [Display(Name = "IsNewCustomerOnly")]
        public bool IsNewCustomerOnly { get; set; }

        [Display(Name = "IsBirthdayCoupon")]
        public bool IsBirthdayCoupon { get; set; }

        [Display(Name = "BirthdayWindow")]
        public CouponBirthdayWindow? BirthdayWindow { get; set; }

        [Display(Name = "Currency")]
        [MaxLength(10)]
        public string Currency { get; set; }

        // Navigation
        public virtual ICollection<CouponProduct> CouponProducts { get; set; }
        public virtual ICollection<CouponCategory> CouponCategories { get; set; }
        public virtual ICollection<CouponRedemption> CouponRedemptions { get; set; }

        // Transient for Admin UI - comma separated ids
        [NotMapped]
        public string ProductIdsCsv { get; set; }

        [NotMapped]
        public string CategoryIdsCsv { get; set; }

        // Backward compat helper - effective discount type if legacy fields used
        [NotMapped]
        public CouponDiscountType EffectiveDiscountType
        {
            get
            {
                if (IsFreeShipping) return CouponDiscountType.FreeShipping;
                if (DiscountType != CouponDiscountType.FixedAmount || DiscountType != 0) return DiscountType;
                // Infer from legacy fields if DiscountType not explicitly set
                if (DiscountPercentage > 0 && Discount == 0) return CouponDiscountType.Percentage;
                if (Discount > 0) return CouponDiscountType.FixedAmount;
                return DiscountType;
            }
        }
    }
}