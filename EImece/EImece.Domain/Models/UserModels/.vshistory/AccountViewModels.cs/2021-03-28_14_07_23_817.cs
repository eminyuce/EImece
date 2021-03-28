using EImece.Domain.Services;
using Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EImece.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.EmailRequired))]
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.NotValidEmailAddress))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.RememberMe))]
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Code))]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.RememberThisBrowser))]
        public bool RememberBrowser { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.RememberMe))]
        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.EmailRequired))]
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.NotValidEmailAddress))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.EmailRequired))]
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.NotValidEmailAddress))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.PasswordRequired))]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Password))]
        public string Password { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.RememberMe))]
        public bool RememberMe { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.AnswerSecurityQuestion))]
        public string Captcha { get; set; }
    }

    public class RegisterViewModel
    {
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.NotValidEmailAddress))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.EmailRequired))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.PasswordRequired))]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.PasswordStringLength), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Password))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ConfirmPassword))]
        [Compare("Password", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.PasswordAndConfirmationPasswordDoNotMatch))]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.FirstName))]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.LastName2))]
        public string LastName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.PhoneNumber))]
        public string PhoneNumber { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IsPermissionGrantedDescription))]
        public bool IsPermissionGranted { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.AnswerSecurityQuestion))]
        public string Captcha { get; set; }

        public ApplicationUser GetUser()
        {
            var user = new ApplicationUser()
            {
                UserName = this.Email,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
            };
            return user;
        }
    }

    public class RegisterViewModelForAdmin
    {
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.NotValidEmailAddress))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.EmailRequired))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.PasswordRequired))]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.PasswordStringLength), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Password))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ConfirmPassword))]
        [Compare("Password", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.PasswordAndConfirmationPasswordDoNotMatch))]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.FirstName))]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.MandatoryField))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.LastName))]
        public string LastName { get; set; }

        public ApplicationUser GetUser()
        {
            var user = new ApplicationUser()
            {
                UserName = this.Email,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
            };
            return user;
        }
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.EmailRequired))]
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.NotValidEmailAddress))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Password))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ConfirmPassword))]
        [Compare("Password", ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.PasswordAndConfirmationPasswordDoNotMatch))]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.EmailRequired))]
        [EmailAddress(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.NotValidEmailAddress))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        public string Email { get; set; }
    }
}