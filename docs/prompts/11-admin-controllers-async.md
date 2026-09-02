# Convert admin controllers to async

- **Captured:** 2026-08-10 4:55:20 PM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

You are working on the open-source e-commerce project EImece (ASP.NET MVC 5 + Entity Framework 6 + .NET Framework 4.8.1).

Goal: Convert Admin area controllers from synchronous to asynchronous *WITHOUT changing any business logic*.

### STRICT RULES

1. *NO business logic changes allowed.*
   - Do not change any conditions, calculations, filters, sorting, validation rules, redirects, TempData, ViewBag, ModelState, or data transformations.
   - Do not refactor, optimize, or “improve” any logic.
   - The only allowed changes are making the call chain async (adding async/await, CancellationToken, and using existing or new *Async methods that behave identically).

2. Controllers are in EImece/Areas/Admin/Controllers/.

3. Change action signatures from:
   public ActionResult Method(...)
   to
   public async Task<ActionResult> Method(CancellationToken cancellationToken = default, ...)

4. Pass CancellationToken down the call chain (controller → service → repository) when the method already supports it or when you create a pure async twin.

5. Use await ...ConfigureAwait(false) only in services and repositories.  
   Do *not* use ConfigureAwait(false) in controller actions.

6. Prefer already-existing async service methods.  
   If a service method is still synchronous, create a new *Async method that is a pure 1:1 async version of the original (same parameters, same return data, same logic, just async).

7. Replace only the blocking calls:
   - .ToList() → await ...ToListAsync(cancellationToken)
   - .FirstOrDefault() → await ...FirstOrDefaultAsync(...)
   - .Count() → await ...CountAsync(...)
   - .SaveChanges() → await ...SaveChangesAsync(...)
   - Remove any .Result / .Wait().

8. Keep all attributes ([Authorize], [HttpPost], [ValidateAntiForgeryToken], etc.) exactly as they are.

9. Child actions marked [ChildActionOnly] *must remain synchronous* (MVC 5 limitation).

10. Preserve every log statement, error handling, redirect, and return statement exactly.

11. Do not change any public storefront controllers.

12. Follow the exact async style already used in:
    - EImece/Controllers/ProductsController.cs
    - EImece.Domain/Services/ProductService.cs (async methods)
    - EImece/docs/ASYNC_AWAIT_GUIDE.md

### Conversion order (one controller at a time)

1. ProductsController (Admin)
2. OrdersController
3. CustomersController
4. ProductCategoriesController
5. DashboardController
6. BrandsController
7. Media / Images related controllers
8. Settings / AdminSettingsController
9. Remaining controllers

### Expected change pattern (business logic untouched)

```csharp
// BEFORE
public ActionResult Index(int page = 1)
{
    var model = ProductService.GetAdminPageList(categoryId, search, lang);
    return View(model);
}

// AFTER (only signature + await, logic identical)
public async Task<ActionResult> Index(CancellationToken cancellationToken, int page = 1)
{
    var model = await ProductService.GetAdminPageListAsync(categoryId, search, lang, cancellationToken);
    return View(model);
}
