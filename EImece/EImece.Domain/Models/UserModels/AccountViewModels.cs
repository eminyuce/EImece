﻿using EImece.Domain.Services;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EImece.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Email))]
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

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.RememberMe))]
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Code))]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.RememberThisBrowser))]
        public bool RememberBrowser { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.RememberMe))]
        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [EmailAddress]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Email))]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Email))]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Password))]
        public string Password { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.RememberMe))]
        public bool RememberMe { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.AnswerSecurityQuestion))]
        public string Captcha { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Email))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.PasswordRequired))]
        [StringLength(100, ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.PasswordStringLength), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Password))]
        public string Password { get; set; }

        //   [DataType(DataType.Password)]
        //    [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ConfirmPassword))]
        //    [Compare("Password", ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.PasswordAndConfirmationPasswordDoNotMatch))]
        //  public string ConfirmPassword { get; set; }

        [Required]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.FirstName))]
        public string FirstName { get; set; }

        [Required]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.LastName))]
        public string LastName { get; set; }

        [Required]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.PhoneNumber))]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.BirthDate))]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM.dd.yyyy}")]
        public DateTime BirthDate { get; set; }

        public bool IsPermissionGranted { get; set; }
        //[Required]
        //[Display(Name = "User Name")]
        //public string UserName { get; set; }

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
        [Required]
        [EmailAddress]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Email))]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Password))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.ConfirmPassword))]
        [Compare("Password", ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.PasswordAndConfirmationPasswordDoNotMatch))]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Email))]
        public string Email { get; set; }
    }
}