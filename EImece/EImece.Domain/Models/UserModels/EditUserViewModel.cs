using Resources;
using System;
using System.ComponentModel.DataAnnotations;

namespace EImece.Models
{
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
        }

        // Allow Initialization with an instance of ApplicationUser:

        public string Id { get; set; }

        [Required]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.FirstName))]
        public string FirstName { get; set; }

        [Required]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.LastName))]
        public string LastName { get; set; }

        [Required]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        public string Email { get; set; }

        public virtual String Role { get; set; }

        /// <summary>
        /// Whether TOTP authenticator 2FA is enabled for this user.
        /// </summary>
        public bool AuthenticatorEnabled { get; set; }

        /// <summary>
        /// True when Identity lockout end is still in the future.
        /// </summary>
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// Multi-line summary for admin grids (phone, address, company, etc.).
        /// </summary>
        public string DetailNote { get; set; }

        public void AppendDetailLine(string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var line = string.IsNullOrWhiteSpace(label)
                ? value.Trim()
                : label.Trim() + ": " + value.Trim();

            if (string.IsNullOrEmpty(DetailNote))
            {
                DetailNote = line;
                return;
            }

            DetailNote += Environment.NewLine + line;
        }

        public void AppendDetailBlock(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var block = value.Trim();
            if (string.IsNullOrEmpty(DetailNote))
            {
                DetailNote = block;
                return;
            }

            DetailNote += Environment.NewLine + block;
        }
    }
}