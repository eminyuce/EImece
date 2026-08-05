using System.Security.Claims;
using EImece.Domain.Core.Identity;
using EImece.Domain.Core.Services;
using EImece.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EImece.Web.Areas.Customers.Controllers;

[Area("Customers")]
[Authorize(Policy = AuthPolicies.CustomerOnly)]
public sealed class HomeController : Controller
{
    private readonly IStorefrontService _storefront;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(IStorefrontService storefront, UserManager<ApplicationUser> userManager)
    {
        _storefront = storefront;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? user?.Id ?? string.Empty;

        IReadOnlyList<EImece.Domain.Core.Entities.Order> orders = [];
        if (!string.IsNullOrWhiteSpace(userId))
        {
            orders = await _storefront.GetOrdersForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        }

        var model = new CustomerAccountViewModel
        {
            UserName = user?.UserName ?? User.Identity?.Name,
            Email = user?.Email,
            Orders = orders.Select(o => new OrderListItemViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                PaymentStatus = o.PaymentStatus,
                Price = o.Price,
                CreatedDate = o.CreatedDate,
                DeliveryDate = o.DeliveryDate,
                ShipmentTrackingNumber = o.ShipmentTrackingNumber,
                ProductCount = o.OrderProducts?.Count ?? 0
            }).ToList()
        };

        ViewData["Title"] = "Hesabım";
        return View(model);
    }
}
