using EImece.Domain.Core.Data;
using EImece.Domain.Core.Identity;
using EImece.Web.Configuration;
using EImece.Web.Helpers;
using EImece.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class UsersController : BaseAdminController
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole> _roles;

    public UsersController(
        IOptions<EImeceOptions> siteOptions,
        UserManager<ApplicationUser> users,
        RoleManager<IdentityRole> roles)
        : base(siteOptions)
    {
        _users = users;
        _roles = roles;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 25, string? sort = null, string? sortDir = null, CancellationToken cancellationToken = default)
    {
        var grid = GridQuery(search, page, pageSize, sort, sortDir);
        var query = _users.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(grid.Search))
        {
            query = query.Where(u => (u.Email != null && u.Email.Contains(grid.Search))
                || (u.UserName != null && u.UserName.Contains(grid.Search))
                || (u.FirstName != null && u.FirstName.Contains(grid.Search)));
        }

        query = (grid.Sort?.ToLowerInvariant()) switch
        {
            "email" => grid.SortDir == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
            "firstname" => grid.SortDir == "asc" ? query.OrderBy(u => u.FirstName) : query.OrderByDescending(u => u.FirstName),
            "lastname" => grid.SortDir == "asc" ? query.OrderBy(u => u.LastName) : query.OrderByDescending(u => u.LastName),
            "confirmed" => grid.SortDir == "asc" ? query.OrderBy(u => u.EmailConfirmed) : query.OrderByDescending(u => u.EmailConfirmed),
            _ => query.OrderBy(u => u.Email)
        };

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query.ApplyPaging(grid)
            .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName, u.EmailConfirmed })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var rows = items.Select(x => (IReadOnlyList<string?>)new string?[]
        {
            x.Id, x.Email, x.FirstName, x.LastName, x.EmailConfirmed ? "Evet" : "Hayır"
        });

        return EntityList(BuildList("Kullanıcılar", "Users",
            new[] { "Id", "Email", "FirstName", "LastName", "Confirmed" }, rows, grid.Search,
            showCreate: false, editAction: "Edit", totalCount: total, grid: grid));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdAsync(id).ConfigureAwait(false);
        if (user is null) return NotFound();

        return View(new AdminUserEditViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Email = user.Email ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminUserEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _users.FindByIdAsync(model.Id).ConfigureAwait(false);
        if (user is null) return NotFound();

        user.FirstName = model.FirstName.Trim();
        user.LastName = model.LastName.Trim();
        user.Email = model.Email.Trim();
        user.UserName = model.Email.Trim();
        user.NormalizedEmail = _users.NormalizeEmail(user.Email);
        user.NormalizedUserName = _users.NormalizeName(user.UserName);

        var result = await _users.UpdateAsync(user).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }
            return View(model);
        }

        SetTempStatus("Kullanıcı güncellendi");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ChangePassword(string id)
    {
        var user = await _users.FindByIdAsync(id).ConfigureAwait(false);
        if (user is null) return NotFound();

        return View(new AdminChangePasswordViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(AdminChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _users.FindByIdAsync(model.UserId).ConfigureAwait(false);
        if (user is null) return NotFound();

        var token = await _users.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
        var result = await _users.ResetPasswordAsync(user, token, model.NewPassword).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }
            return View(model);
        }

        SetTempStatus("Şifre güncellendi");
        return RedirectToAction(nameof(Edit), new { id = model.UserId });
    }

    [HttpGet]
    public async Task<IActionResult> UserRoles(string id)
    {
        var user = await _users.FindByIdAsync(id).ConfigureAwait(false);
        if (user is null) return NotFound();

        var roles = await _users.GetRolesAsync(user).ConfigureAwait(false);
        return View(new AdminUserRolesViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            IsAdmin = roles.Contains(RoleNames.Admin),
            IsCustomer = roles.Contains(RoleNames.Customer)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserRoles(string userId, bool isAdmin, bool isCustomer)
    {
        var user = await _users.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null) return NotFound();

        await EnsureRoleExists(RoleNames.Admin).ConfigureAwait(false);
        await EnsureRoleExists(RoleNames.Customer).ConfigureAwait(false);

        var current = await _users.GetRolesAsync(user).ConfigureAwait(false);
        if (current.Contains(RoleNames.Admin) != isAdmin)
        {
            if (isAdmin) await _users.AddToRoleAsync(user, RoleNames.Admin).ConfigureAwait(false);
            else await _users.RemoveFromRoleAsync(user, RoleNames.Admin).ConfigureAwait(false);
        }

        if (current.Contains(RoleNames.Customer) != isCustomer)
        {
            if (isCustomer) await _users.AddToRoleAsync(user, RoleNames.Customer).ConfigureAwait(false);
            else await _users.RemoveFromRoleAsync(user, RoleNames.Customer).ConfigureAwait(false);
        }

        SetTempStatus("Roller güncellendi");
        return RedirectToAction(nameof(Edit), new { id = userId });
    }

    private async Task EnsureRoleExists(string roleName)
    {
        if (!await _roles.RoleExistsAsync(roleName).ConfigureAwait(false))
        {
            await _roles.CreateAsync(new IdentityRole(roleName)).ConfigureAwait(false);
        }
    }
}
