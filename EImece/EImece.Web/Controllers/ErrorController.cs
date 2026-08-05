using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class ErrorController : BaseController
{
    public ErrorController(IOptions<EImeceOptions> siteOptions) : base(siteOptions) { }

    public IActionResult Index()
        => View(new ErrorPageViewModel { StatusCode = 500, Title = "Hata" });

    public new IActionResult BadRequest()
        => ErrorView(400, "Geçersiz istek");

    public new IActionResult NotFound()
        => ErrorView(404, "Sayfa bulunamadı");

    public IActionResult Forbidden()
        => ErrorView(403, "Erişim engellendi");

    public new IActionResult Unauthorized()
        => ErrorView(401, "Yetkisiz erişim");

    public IActionResult InternalServerError()
        => ErrorView(500, "Sunucu hatası");

    public IActionResult MethodNotAllowed()
        => ErrorView(405, "İzin verilmeyen yöntem");

    private IActionResult ErrorView(int statusCode, string title)
    {
        Response.StatusCode = statusCode;
        var model = new ErrorPageViewModel
        {
            StatusCode = statusCode,
            Title = title,
            RequestedUrl = $"{Request.Path}{Request.QueryString}"
        };

        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return PartialView("ErrorPartial", model);
        }

        return View("ErrorPage", model);
    }
}
