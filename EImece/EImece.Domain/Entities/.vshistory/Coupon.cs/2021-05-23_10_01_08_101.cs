using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Helpers;
using Newtonsoft.Json;
using Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace EImece.Domain.Entities
{
    public class Coupon : BaseEntity
    {
        [Required(ErrorMessage = "Code is required")]
        public string Code { get; set; }
        public int DiscountPercentage { get; set; }
        public int Discount  { get; set; }
      
        public DateTime StartDate { get; set; }
     
        public DateTime EndDate { get; set; }

        [DisplayName("StartDate")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "StartDate is required")]
        public string StartDateStr { get; set; }
        [DisplayName("EndDate")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "EndDate is required")]
        public string EndDateStr { get; set; }
    }
}
