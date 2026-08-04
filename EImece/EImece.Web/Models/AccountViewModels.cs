using System.ComponentModel.DataAnnotations;

namespace EImece.Web.Models;

public sealed class LoginViewModel
{
    [Required]
    [Display(Name = "Email or user name")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
