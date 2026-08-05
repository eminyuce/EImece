using EImece.Domain.Core.Identity;
using EImece.Web.Configuration;
using EImece.Web.Helpers;
using EImece.Web.Infrastructure.Routing;
using EImece.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = AuthPolicies.AdminOrEditor)]
public abstract class BaseAdminController : Controller
{
    protected EImeceOptions SiteOptions { get; }

    protected BaseAdminController(IOptions<EImeceOptions> siteOptions)
    {
        SiteOptions = siteOptions.Value;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewData["AdminUser"] = User.Identity?.Name;
        base.OnActionExecuting(context);
    }

    protected void SetAdminCulture(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return;
        }

        Response.Cookies.Append(
            RouteConstants.AdminCultureCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, Path = "/" });
    }

    protected void SetTempStatus(string message, bool isError = false)
    {
        TempData[isError ? "Error" : "Status"] = message;
    }

    protected IActionResult AdminPlaceholder(string title, string message)
    {
        ViewData["Title"] = title;
        ViewData["Message"] = message;
        return View("~/Areas/Admin/Views/Shared/Placeholder.cshtml");
    }

    protected IActionResult EntityList(AdminListViewModel model)
        => View("~/Areas/Admin/Views/Shared/EntityList.cshtml", model);

    protected AdminGridQuery GridQuery(string? search, int page = 1, int pageSize = 25, string? sort = null, string? sortDir = null)
        => AdminGridQuery.From(search, page, pageSize, sort, sortDir);

    protected static AdminListViewModel BuildList(
        string title,
        string controllerName,
        IReadOnlyList<string> columns,
        IEnumerable<IReadOnlyList<string?>> rows,
        string? search = null,
        string? notice = null,
        bool showCreate = true,
        string editAction = "SaveOrEdit",
        int totalCount = -1,
        AdminGridQuery? grid = null,
        string? ajaxDeleteAction = null,
        bool showExport = false)
    {
        var rowList = rows.ToList();
        grid ??= AdminGridQuery.From(search);
        return new AdminListViewModel
        {
            Title = title,
            ControllerName = controllerName,
            Columns = columns,
            Rows = rowList,
            TotalCount = totalCount >= 0 ? totalCount : rowList.Count,
            Search = search ?? grid.Search,
            Notice = notice,
            ShowCreateLink = showCreate,
            EditAction = editAction,
            Page = grid.Page,
            PageSize = grid.PageSize,
            Sort = grid.Sort,
            SortDir = grid.SortDir,
            AjaxDeleteAction = ajaxDeleteAction,
            ShowExportLink = showExport
        };
    }
}
