# Async/await in EImece (ASP.NET MVC 5 + Entity Framework 6)

This guide documents how the product listing path was converted from synchronous
controllers and synchronous EF6 queries to an unbroken async chain, and the rules to follow
when converting the remaining actions.

The worked example is `ProductsController.Index` — a listing page that fetches products from
the database and returns a view.

---

## Why async frees the thread

ASP.NET serves each request on a thread from a fixed-size thread pool. In the synchronous
version, `ToList()` sends the query to SQL Server and then blocks: the thread sits in a wait
state doing nothing until the last row comes back. If a query takes 200 ms, that thread is
unavailable for 200 ms even though it is not computing anything.

The pool grows only slowly (roughly one or two threads per second beyond the minimum), so a
traffic burst against slow queries produces a queue of requests waiting for threads, not for
the database. Requests start timing out while CPU sits nearly idle.

With `await ToListAsync()`, the query is issued and the method returns to the caller at the
first `await`. ASP.NET returns the thread to the pool and it serves other requests. When SQL
Server answers, an I/O completion callback schedules the rest of the method on a pool thread
again — with the request's `HttpContext` and culture restored by the
`AspNetSynchronizationContext`. Total latency for one request is unchanged; concurrency at a
given thread count goes up, because thread occupancy now tracks CPU work rather than database
wait time.

The important corollary: **async only helps if nothing in the chain blocks.** A single
`.Result`, `.Wait()`, or synchronous `ToList()` in a service or repository puts the blocking
wait back and also risks deadlock (see pitfalls).

---

## Key changes

| Layer | Before | After |
|---|---|---|
| Controller | `ActionResult Index(...)` | `async Task<ActionResult> Index(CancellationToken, ...)` |
| Service | `ProductIndexViewModel GetMainPageProducts(...)` | `Task<ProductIndexViewModel> GetMainPageProductsAsync(...)` |
| Repository | `PaginatedList<Product> GetActiveProducts(...)` | `Task<PaginatedList<Product>> GetActiveProductsAsync(...)` |
| Paging | `Count()` + deferred `IQueryable` | `await CountAsync()` + `await ToListAsync()` |
| Awaits below the controller | n/a | `.ConfigureAwait(false)` |

The synchronous methods were kept in place. They still have callers (the admin area, RSS, the
category pages), so the conversion is additive per call path rather than a single cut-over.

---

## Synchronous version (for reference)

Controller. `ProductsController` had no `Index` action before this change — the synchronous
`GetMainPageProducts` service method existed but had no caller, so the listing page was
unreachable. This is the action it was written for, and it is the shape the rest of the public
controllers use today:

```csharp
public ActionResult Index(int page = 1)
{
    var products = ProductService.GetMainPageProducts(page, CurrentLanguage);
    return View(products);
}
```

Service — `EImece.Domain/Services/ProductService.cs`, unchanged and still present:

```csharp
public ProductIndexViewModel GetMainPageProducts(int page, int language)
{
    var cacheKey = $"GetMainPageProducts-{page}-{language}";

    return DataCachingProvider.GetOrAdd(cacheKey, () =>
    {
        var result = new ProductIndexViewModel();
        int pageSize = AppConfig.RecordPerPage;
        result.CompanyName = SettingService.GetSettingObjectByKey(Constants.CompanyName);
        result.MainPageMenu = MenuService.GetActiveBaseContentsFromCache(true, language)
            .FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
        result.ProductMenu = MenuService.GetActiveBaseContentsFromCache(true, language)
            .FirstOrDefault(r1 => r1.MenuLink.Equals("products-index", StringComparison.InvariantCultureIgnoreCase));

        var items = ProductRepository.GetActiveProducts(page, pageSize, language);
        result.Products = items;
        result.Tags = TagService.GetActiveBaseEntities(true, language);
        return result;
    }, AppConfig.CacheMediumSeconds);
}
```

Repository, ending in the blocking paging helper:

```csharp
public static PaginatedList<T> ToPaginatedList<T>(this IQueryable<T> query, int pageIndex, int pageSize)
{
    int totalCount = query.Count();                                       // blocks
    IQueryable<T> collection = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

    return new PaginatedList<T>(collection, pageIndex, pageSize, totalCount); // blocks again in AddRange
}
```

Note the second blocking call is easy to miss: `PaginatedList<T>` derives from `List<T>` and its
constructor calls `AddRange(source)`. Handing it a deferred `IQueryable` means the page query is
executed *inside the constructor*, synchronously.

---

## Async version

### Controller — `EImece/Controllers/ProductsController.cs`

```csharp
[CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
public async Task<ActionResult> Index(CancellationToken cancellationToken, int page = 1)
{
    Logger.Info($"Entering Index action with page: {page}, language: {CurrentLanguage}");

    if (page < 1)
    {
        Logger.Error($"Invalid page number: {page}. Returning BadRequest status.");
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    }

    // No ConfigureAwait(false) here on purpose: the continuation needs HttpContext.Current
    // and the request culture to render the view.
    var products = await ProductService.GetMainPageProductsAsync(page, CurrentLanguage, cancellationToken);

    products.Page = page;
    products.RecordPerPage = AppConfig.RecordPerPage;

    return View(products);
}
```

### Service — `EImece.Domain/Services/ProductService.cs`

```csharp
public async Task<ProductIndexViewModel> GetMainPageProductsAsync(int page, int language, CancellationToken cancellationToken = default(CancellationToken))
{
    var result = new ProductIndexViewModel();
    int pageSize = AppConfig.RecordPerPage;

    result.CompanyName = await SettingService.GetSettingObjectByKeyAsync(Constants.CompanyName).ConfigureAwait(false);

    var menus = await MenuService.GetActiveBaseContentsFromCacheAsync(true, language).ConfigureAwait(false);
    result.MainPageMenu = menus.FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
    result.ProductMenu = menus.FirstOrDefault(r1 => r1.MenuLink.Equals("products-index", StringComparison.InvariantCultureIgnoreCase));

    result.Products = await ProductRepository.GetActiveProductsAsync(page, pageSize, language, cancellationToken).ConfigureAwait(false);
    result.Tags = await TagService.GetActiveBaseEntitiesFromCacheAsync(true, language).ConfigureAwait(false);

    return result;
}
```

Two things changed beyond adding `await`:

- The menu list is fetched once into a local instead of twice, which the synchronous version did
  because both calls were cheap cache hits.
- The composed view model is no longer wrapped in a single cache entry. A cache entry is shared
  by all concurrent callers, so it cannot honour a per-request `CancellationToken`; the
  language-scoped parts keep their own single-flight caches, the paged query runs per request,
  and whole-page caching is left to `CustomOutputCache`.

### Repository — `EImece.Domain/Repositories/ProductRepository.cs`

```csharp
public async Task<PaginatedList<Product>> GetActiveProductsAsync(int pageIndex, int pageSize, int language, CancellationToken cancellationToken = default(CancellationToken))
{
    try
    {
        Expression<Func<Product, object>> includeProperty1 = r => r.ProductFiles;
        Expression<Func<Product, object>> includeProperty2 = r => r.ProductCategory;
        Expression<Func<Product, object>> includeProperty3 = r => r.MainImage;
        Expression<Func<Product, object>> includeProperty4 = r => r.ProductTags.Select(t => t.Tag);
        Expression<Func<Product, object>>[] includeProperties = {
            includeProperty1, includeProperty2, includeProperty4, includeProperty3 };
        Expression<Func<Product, bool>> match = r2 => r2.IsActive && r2.Lang == language;
        Expression<Func<Product, int>> keySelector = t => t.Position;

        return await this.PaginateDescendingAsync(pageIndex, pageSize, keySelector, match, cancellationToken, includeProperties)
                         .ConfigureAwait(false);
    }
    catch (Exception exception)
    {
        Logger.Error(exception, exception.Message);
        throw;
    }
}
```

### Paging primitive — `EImece.Domain/GenericRepository/QueryableExtensions.cs`

This is where the chain reaches the DbContext:

```csharp
public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
    this IQueryable<T> query, int pageIndex, int pageSize, CancellationToken cancellationToken = default(CancellationToken))
{
    int totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

    var items = await query
        .Skip((pageIndex - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken)
        .ConfigureAwait(false);

    return new PaginatedList<T>(items, pageIndex, pageSize, totalCount);
}
```

`items` is a materialised `List<T>`, so `PaginatedList`'s `AddRange` no longer triggers a hidden
synchronous query.

---

## Two actions that were genuinely converted in place

`Index` is new. `SearchProducts` and `Review` already existed as synchronous actions, so they show
the diff on real code.

### `SearchProducts` — a read

```csharp
// before
public ActionResult SearchProducts(String search, int page = 1, int sorting = 0)
{
    ...
    var products = ProductService.SearchProducts(page, pageSize, search, CurrentLanguage, (SortingType)sorting);
    ...
}

// after
public async Task<ActionResult> SearchProducts(String search, CancellationToken cancellationToken, int page = 1, int sorting = 0)
{
    ...
    var products = await ProductService.SearchProductsAsync(page, pageSize, search, CurrentLanguage, (SortingType)sorting, cancellationToken);
    ...
}
```

Note the parameter order: `CancellationToken` has no default, so it must precede the optional
parameters. Model binding fills it in either way, and the route/query string is unaffected.

### `Review` — a write

```csharp
// before
public ActionResult Review(ProductComment productComment)
{
    ...
    var users = ApplicationDbContext.Users.AsQueryable();
    var user = users.Where(u => u.UserName.Equals(productComment.Email, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
    ...
    productCommentService.SaveOrEditEntity(productComment);
    return RedirectToAction("Detail", new { id = productComment.SeoUrl });
}

// after
public async Task<ActionResult> Review(ProductComment productComment)
{
    ...
    var email = productComment.Email.ToStr().Trim();
    var user = await ApplicationDbContext.Users.FirstOrDefaultAsync(u => u.UserName == email);
    ...
    await productCommentService.SaveOrEditEntityAsync(productComment);
    return RedirectToAction("Detail", new { id = productComment.SeoUrl });
}
```

The predicate had to change to be awaited at all: see the `StringComparison` pitfall below. The
sync version threw `NotSupportedException` on the same expression, it just did so on a blocked
thread. `Review` takes no `CancellationToken` on purpose — cancelling a half-written comment
mid-`SaveChangesAsync` is worse than letting it finish.

---

## Most important EF6 async methods

All of these live in `System.Data.Entity.QueryableExtensions`, so the file needs
`using System.Data.Entity;` — not just `System.Linq`. Each has a `CancellationToken` overload.

| Method | Replaces | Notes |
|---|---|---|
| `ToListAsync()` | `ToList()` | The workhorse for list reads |
| `FirstOrDefaultAsync()` | `FirstOrDefault()` | Takes an optional predicate |
| `SingleOrDefaultAsync()` | `SingleOrDefault()` | Still throws if more than one row matches |
| `CountAsync()` / `LongCountAsync()` | `Count()` | Needed for paging totals |
| `AnyAsync()` / `AllAsync()` | `Any()` / `All()` | Cheaper than `CountAsync() > 0` |
| `SumAsync()`, `MinAsync()`, `MaxAsync()`, `AverageAsync()` | aggregates | |
| `ToArrayAsync()`, `ToDictionaryAsync()` | materialisers | |
| `ForEachAsync()` | `foreach` over a query | Streams without buffering |
| `FindAsync(id)` | `Find(id)` | On `DbSet<T>`, not `IQueryable` |
| `SaveChangesAsync()` | `SaveChanges()` | On `DbContext` |
| `LoadAsync()`, `ReloadAsync()` | explicit loading | On `DbEntityEntry` / `DbCollectionEntry` |
| `Database.ExecuteSqlCommandAsync()` | `ExecuteSqlCommand()` | Raw SQL |

Async EF6 operators only work on providers that implement `IDbAsyncQueryProvider`. Calling
`ToListAsync()` on a plain in-memory `IQueryable` (a `List<T>.AsQueryable()` in a unit test)
throws `InvalidOperationException` at runtime, so fake repositories must return real tasks
rather than in-memory queryables.

---

## Common mistakes to avoid

**Blocking on a task.** `.Result`, `.Wait()`, and `.GetAwaiter().GetResult()` in a request undo
the benefit and can deadlock: the continuation needs the `AspNetSynchronizationContext`, which is
occupied by the thread that is blocking on the task. The one legitimate use in this codebase is
`DependencyInjectionConfig` during `Application_Start`, where there is no request context.

**Async over a shared cache.** LazyCache stores a `Lazy<T>` for `GetOrAdd` and an `AsyncLazy<T>`
for `GetOrAddAsync`. If a synchronous reader hits a key an async reader populated, LazyCache
unwraps it internally with `GetAwaiter().GetResult()` — a hidden block. While both spellings of a
method exist, the async cached accessors use their own keys
(`BaseService.AsyncCacheKeySuffix`).

**Forwarding a request token into a shared cache factory.** A single-flight factory result is
awaited by every concurrent caller, so one caller cancelling would fault everyone else's request.
Cache-populating factories pass `CancellationToken.None`; the caller's token goes to
per-request queries.

**`async void`.** Exceptions escape to the `SynchronizationContext` and crash the process instead
of surfacing as a 500. Return `Task` even when there is no value.

**`ConfigureAwait(false)` in a controller.** The continuation after an `await` in an action often
touches `HttpContext.Current`, `Request`, `User`, `TempData`, or the request culture, none of
which are restored without the context. Use it in service/repository code — where it also avoids
a needless hop back onto the request context — and omit it in controllers.

**Forgetting to await.** A bare call to an async method returns a task and the action returns
before the work finishes; on the write path the `DbContext` is disposed at end of request while
`SaveChangesAsync` is still using it. There is no compiler error, only warning CS4014.

**Sharing a `DbContext` across concurrent awaits.** `IEImeceContext` is registered per request.
`Task.WhenAll` over two queries on the same context throws
`NotSupportedException: A second operation started on this context before a previous
asynchronous operation completed`. Await sequentially, or give each parallel branch its own
context.

**`String.Equals(..., StringComparison)` inside a predicate.** Unrelated to async, but it is what
`ProductsController.Review` did: EF6 cannot translate the `StringComparison` overloads and throws
`NotSupportedException` at runtime. Compare with `==` and rely on the database collation.

**Assuming the `CancellationToken` parameter means client disconnect.** In MVC 5,
`CancellationTokenModelBinder` returns `default(CancellationToken)`, and
`TaskAsyncActionDescriptor` then swaps it for a real token that is cancelled when the action's
async timeout elapses (`AsyncManager.Timeout`, 45 seconds by default; `[AsyncTimeout]` changes it,
`[NoAsyncTimeout]` disables the timer). It is a timeout token, not a disconnect token. To react to
a disconnect, link it:

```csharp
using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, Response.ClientDisconnectedToken))
{
    var model = await service.GetAsync(cts.Token);
}
```

**Missing `targetFramework` in `httpRuntime`.** Async actions need
`<httpRuntime targetFramework="4.5" />` or higher, otherwise the request is executed under
ASP.NET 2.0 quirks mode and async completions behave incorrectly. This app sets `4.8.1`.

---

## Scope of the public-site conversion

Every public (non-admin) controller action that hits the database now runs as
`async Task<ActionResult>` (or `Task<JsonResult>`) with an unbroken await chain into EF6 /
ADO.NET async APIs. Sync twins were kept on services and repositories so the admin area and
any remaining callers keep compiling.

| Area | Converted |
|---|---|
| `ProductsController` | `Index`, `Detail`, `Tag`, `SearchProducts`, `AdvancedSearchProducts`, `Review` |
| `ProductCategoriesController` | `Category`, `GetProductCategoryDto` |
| `StoriesController` | `Index`, `Detail`, `Categories`, `Tag` |
| `HomeController` | `Index`, settings/subscriber/contact actions that hit I/O |
| `PagesController` / `InfoController` | `Detail` / `Index` |
| `PaymentController` | cart, checkout, coupon, buy-now, thank-you, Iyzico callbacks |
| `Areas/Customers/HomeController` | full customer area (only controller in the area): profile GET/POST, FAQ, send-message-to-seller GET/POST, orders, order detail, password GET/POST, address info. `LogOff` stays sync (cookie sign-out only). `OnActionExecuting` price gate stays sync because MVC 5 filters cannot await; it uses the LazyCache-backed `GetSettingByKey`. |
| `RssController` / `SiteMapController` | all feed/sitemap actions |
| `AccountController` / `ManageController` | remaining Identity/settings leftovers inside already-async actions |
| `HealthController` | already async |

## What is still synchronous (and why)

| Item | Why it was left alone |
|---|---|
| `[ChildActionOnly]` actions (`Navigation`, `Footer`, `WebSiteLogo`, cart partials, analytics/WhatsApp scripts) | MVC 5 cannot await child actions. Refactor to AJAX/`Html.Partial` with data from the parent before converting. |
| `ImagesController` | Disk read + CPU resize/WebP. Async file I/O would help a little; the resize stays CPU-bound. Output-cached. |
| `AjaxController` city/town/district | In-memory `TurkishRegionService`, no DB. |
| View-only / no-I/O shells | `ErrorController`, `UnderConstructionController`, `RobotController`, `LogOff`, `CargoTracking`, `NoSuccessForYourOrder`, Account GET shells, `Languages`, cache dump, order-confirmation email helper |
| Admin area (`Areas/Admin`) | Explicitly out of scope for this pass |
| Email send helpers (`RazorEngineHelper`, `EmailSender`) | Still sync SMTP/template; called from a few POST actions after the DB work has already been awaited |

When converting a `[ChildActionOnly]`, remove the child-action usage first — changing the signature to `async Task<ActionResult>` without that refactor will throw at runtime.
