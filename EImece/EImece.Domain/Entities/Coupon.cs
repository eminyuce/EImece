using Newtonsoft.Json;
using Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class Coupon : BaseEntity
    {
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

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.StartDate))]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [NotMapped]
        public string StartDateStr { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.EndDate))]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [NotMapped]
        public string EndDateStr { get; set; }
    }
}