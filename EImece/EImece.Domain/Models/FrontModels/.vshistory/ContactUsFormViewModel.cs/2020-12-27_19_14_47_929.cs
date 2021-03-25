using EImece.Domain.Models.Enums;
using Resources;
using System;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Models.FrontModels
{
    public class ContactUsFormViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ContactUsNameErrorMessage))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ContactUsName))]
        public String Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ContactUsEmailErrorMessage))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ContactUsEmail))]
        public String Email { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ContactUsCompanyName))]
        public String CompanyName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ContactUsPhoneErrorMessage))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ContactUsPhone))]
        public String Phone { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ContactUsAddress))]
        public String Address { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.ContactUsMessageErrorMessage))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ContactUsMessage))]
        public String Message { get; set; }

        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.AnswerSecurityQuestion))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ContactUsCaptcha))]
        public String Captcha { get; set; }

        public String ContactFormType { get; set; }
        public int ItemId { get; set; }
        public EImeceItemType ItemType { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.OrderNumber))]
        public String OrderNumber { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.SelectReasonsLabel))]
        public String Reasons { get; set; }

        public static ContactUsFormViewModel CreateContactUsFormViewModel(string contactFormType, int itemId, EImeceItemType itemType)
        {
            var result = new ContactUsFormViewModel();
            result.ContactFormType = contactFormType;
            result.ItemId = itemId;
            result.ItemType = itemType;
            return result;
        }
    }
}