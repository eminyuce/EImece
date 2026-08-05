using EImece.Domain.Core.Identity;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

[Authorize]
public sealed class ManageController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ManageController(IOptions<EImeceOptions> siteOptions, UserManager<ApplicationUser> userManager)
        : base(siteOptions)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? message)
    {
        var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);
        if (user is null)
        {
            return Challenge();
        }

        var model = new ManageIndexViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = await _userManager.GetPhoneNumberAsync(user).ConfigureAwait(false),
            HasPassword = await _userManager.HasPasswordAsync(user).ConfigureAwait(false),
            StatusMessage = message switch
            {
                "password" => "Şifreniz güncellendi.",
                "error" => "Bir hata oluştu.",
                _ => null
            }
        };

        return View(model);
    }
}
