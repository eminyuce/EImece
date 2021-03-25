using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EImece.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.PasswordRequired))]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.NewPassword))]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ConfirmNewPassword))]
        [Compare("NewPassword", ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.NotMatchesPassword))]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.PasswordRequired))]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.CurrentPassword))]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.PasswordRequired))]
        [StringLength(100, ErrorMessageResourceName = nameof(AdminResource.NewPasswordMinCharLength), ErrorMessageResourceType = typeof(AdminResource), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.NewPassword))]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ConfirmNewPassword))]
        [Compare("NewPassword", ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.NotMatchesPassword))]
        public string ConfirmPassword { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.PhoneNumber))]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.MandatoryField))]
        [Phone]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.PhoneNumber))]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }
}