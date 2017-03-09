using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resources;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.FrontModels
{
    public  class ContactUsFormViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ContactUsNameErrorMessage))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ContactUsName))]
        public String Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ContactUsEmailErrorMessage))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ContactUsEmail))]
        public String Email { get; set; }
 
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ContactUsCompanyName))]
        public String CompanyName { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ContactUsPhone))]
        public String Phone { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ContactUsAddress))]
        public String Address { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ContactUsMessageErrorMessage))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ContactUsMessage))]
        public String Message { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ContactUsCaptchaErrorMessage))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ContactUsCaptcha))]
        public String Captcha { get; set; }
    }
}
