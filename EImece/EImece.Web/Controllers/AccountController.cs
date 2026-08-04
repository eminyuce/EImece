using EImece.Domain.Core.Identity;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

/// <summary>
/// Minimal Account surface for Phase 5 (Login / AdminLogin / Logout / AccessDenied).
/// Full Register / Manage / 2FA UI deferred.
/// </summary>
public sealed class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EImeceOptions _options;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IOptions<EImeceOptions> options,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _options = options.Value;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await FindUserAsync(model.Email).ConfigureAwait(false);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true).ConfigureAwait(false);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {UserId} signed in", user.Id);
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            // Storefront login historically lands in Customers area when role matches.
            if (await _userManager.IsInRoleAsync(user, RoleNames.Customer).ConfigureAwait(false))
            {
                return RedirectToAction("Index", "Home", new { area = "Customers" });
            }

            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked out. Try again later.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AdminLogin(string? returnUrl = null)
    {
        if (!_options.AdminLoginEnabled && !_options.BypassAdminAuth)
        {
            return RedirectToAction("Index", "Home");
        }

        if (_options.BypassAdminAuth)
        {
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminLogin(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!_options.AdminLoginEnabled && !_options.BypassAdminAuth)
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await FindUserAsync(model.Email).ConfigureAwait(false);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid admin login attempt.");
            return View(model);
        }

        // Customers cannot use AdminLogin (legacy parity).
        if (await _userManager.IsInRoleAsync(user, RoleNames.Customer).ConfigureAwait(false)
            && !await _userManager.IsInRoleAsync(user, RoleNames.Admin).ConfigureAwait(false)
            && !await _userManager.IsInRoleAsync(user, RoleNames.NormalUser).ConfigureAwait(false))
        {
            ModelState.AddModelError(string.Empty, "Customer accounts cannot access admin login.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true).ConfigureAwait(false);

        if (result.Succeeded)
        {
            var isAdminOrEditor =
                await _userManager.IsInRoleAsync(user, RoleNames.Admin).ConfigureAwait(false)
                || await _userManager.IsInRoleAsync(user, RoleNames.NormalUser).ConfigureAwait(false);

            if (!isAdminOrEditor)
            {
                await _signInManager.SignOutAsync().ConfigureAwait(false);
                ModelState.AddModelError(string.Empty, "You are not authorized for the admin area.");
                return View(model);
            }

            _logger.LogInformation("Admin user {UserId} signed in", user.Id);
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        ModelState.AddModelError(string.Empty, "Invalid admin login attempt.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync().ConfigureAwait(false);
        _logger.LogInformation("User signed out");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (!string.IsNullOrEmpty(remoteError))
        {
            TempData["Error"] = $"External provider error: {remoteError}";
            return RedirectToAction(nameof(Login));
        }

        var info = await _signInManager.GetExternalLoginInfoAsync().ConfigureAwait(false);
        if (info is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true).ConfigureAwait(false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        TempData["Error"] = "External login is not linked to an existing account. Account linking UI arrives in a later phase.";
        return RedirectToAction(nameof(Login));
    }

    private async Task<ApplicationUser?> FindUserAsync(string emailOrUserName)
    {
        var user = await _userManager.FindByEmailAsync(emailOrUserName).ConfigureAwait(false);
        return user ?? await _userManager.FindByNameAsync(emailOrUserName).ConfigureAwait(false);
    }
}
