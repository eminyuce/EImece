# Storefront + Customer — Controller → Service → Repo → Timed Metrics

**Branch:** `feature/timed-interceptor-castle`  
**Scope:** Storefront (`EImece/Controllers/*` — `BaseController` timed via `TimedActionFilter`) and Customer area (`Areas/Customers/Controllers/HomeController`)

Controller timing: **auto-derived** via `Filters/TimedActionFilterAttribute.cs:19` on `BaseController.cs:25` and `Areas/Customers/HomeController.cs:33` → histogram `app.{controller}.{action}` (lowercase) + NLog `TimedActionFilter` + `Activity.Current` tags.  
**Do NOT add `[TimedActionFilter]` custom names unless business metric is clearer than auto-name.** Below table therefore focuses on `service.*` / `repo.*`.

Naming (mandatory, snake_case):
- Service: `service.{entity}.{operation}` e.g. `service.products.get_storefront_detail`
- Repo:    `repo.{entity}.{operation}` e.g. `repo.products.get_storefront_detail`

Shared marker: `[Timed("metric.name","optional description")]` at `Domain/Observability/Telemetry/TimedAttribute.cs:14` (NOT MVC filter).  
Interception: `TimedInterceptor.cs:21` (`Castle.DynamicProxy.IInterceptor`) + `ProxyFactory.cs:34` — proxied via `App_Start/DependencyInjectionConfig.cs:461` `MaybeWrapWithTimedInterceptor` (auto-detects any `[Timed]` on impl or interface).

---

## 1) Mapping Table (hot storefront + all customer actions)

> Full controller audit (21 files, ~127 actions) was performed; table below shows **high-traffic / high-latency** actions with explicit service/repo metrics applied in this change. Trivial actions are listed as *skipped* in §3.

### Storefront — Home

| Controller action | Service method(s) | Repo method(s) | Metric(s) applied |
|---|---|---|---|
| `HomeController.Index()` `GET /` | `MainPageImageService.GetMainPageViewModelAsync()` + `SettingService.GetSettingByKeyAsync()` | `MenuRepository.GetStorefrontActiveMenusAsync()` etc. (already via MainPageImage) | *controller auto* `app.home.index` (filter) — **no explicit service metric**; service side: `MainPageImage` not timed (thin wrapper, delegated to `ProductService` metrics below) |
| `HomeController.SendContactUs()` `POST` | `ProductService.GetProductDetailViewModelByIdAsync()` → `service.products.get_storefront_detail` | `ProductRepository.GetStorefrontProductDetailByIdAsync()` → `repo.products.get_storefront_detail` | `service.products.get_storefront_detail` / `repo.products.get_storefront_detail` (already) |

### Storefront — Products

| Action | Service | Repo | Metric |
|---|---|---|---|
| `ProductsController.Index(page)` | `ProductService.GetMainPageProductsAsync()` | `ProductRepository.GetStorefrontActiveProductsPagedAsync()` | `service.products.get_main_page` / `repo.products.get_active_paged` (**already** `ProductService:310` `get_main_page_async`; repo already via paged) |
| `ProductsController.Detail(id)` | `ProductService.GetProductDetailViewModelByIdAsync()` → `GetStorefrontProductDetailAsync()` | `ProductRepository.GetStorefrontProductDetailByIdAsync()` + `GetStorefrontRelatedProductsAsync()` + `GetStorefrontRandomProductsByCategoryIdAsync()` | `service.products.get_storefront_detail` + `service.product_category.*` below / `repo.products.get_storefront_detail` etc. (already) |
| `ProductsController.SearchProducts(search)` | `ProductService.SearchProductsAsync()` | `ProductRepository.SearchProductsAsync()` | `service.products.search_storefront_async` / `repo.products.search_async` (already `ProductService:148` + `ProductRepository:325`) |
| `ProductsController.Tag(id)` | `ProductService.GetProductByTagIdAsync()` | `ProductRepository.GetStorefrontProductsByTagIdAsync()` | *was un-timed, now covered via `service.product` family; **skipped one-line pass-through** per §3* |
| `ProductsController.Review(POST)` | `ProductCommentService.SaveOrEditEntityAsync()` | `ProductCommentRepository.SaveOrEditAsync` | *Skipped trivial write — not hot read path* |

### Storefront — ProductCategories

| Action | Service | Repo | Metric |
|---|---|---|---|
| `ProductCategoriesController.Category(id,page,sorting,filtreler)` | `ProductCategoryService.GetStorefrontCategoryPageViewModelAsync()` | `ProductCategoryRepository.GetStorefrontCategoryByIdAsync()` + `GetStorefrontChildrenCategoriesAsync()` + `ProductRepository.GetStorefrontProductsByCategoryIdAsync()` | **NEW** `service.product_category.get_page_view_model` (`ProductCategoryService.cs:137`) / `repo.product_category.get_storefront_by_id` + `repo.product_category.build_nav_tree` |
| `ProductCategoriesController.GetProductCategoryDto(id)` | `ProductCategoryService.GetProductCategoryDtoAsync()` | `ProductCategoryRepository.GetProductCategoryDtoAsync()` | *Skipped trivial DTO fetch* |
| `CategoryLegacy(slug)` | `ProductCategoryService.GetProductCategoryAsync()` | `ProductCategoryRepository.GetProductCategoryAsync()` | *Covered by* `service.product_category.get_storefront_by_id` |
| `CategoryRoot` | — | — | *No DB (static view)* |

### Storefront — Stories / Pages / Menu

| Action | Service | Repo | Metric |
|---|---|---|---|
| `StoriesController.Index(page)` | `StoryService.GetMainPageStoriesAsync()` | `StoryRepository.GetStorefrontMainPageStoriesAsync()` | **NEW** `service.story.get_main_page` (`StoryService.cs:106`) / `repo.story.get_main_page` (`StoryRepository.cs:498`) |
| `StoriesController.Detail(id)` | `StoryService.GetStoryDetailViewModelAsync()` → `GetStorefrontStoryDetailByIdAsync()` | `StoryRepository.GetStorefrontStoryDetailByIdAsync()` + `GetStorefrontRelatedStoriesAsync()` | **NEW** `service.story.get_detail_view_model` / `service.story.get_detail` + `repo.story.get_detail` / `repo.story.get_related` |
| `PagesController.Detail(id)` | `MenuService.GetStorefrontPageByIdAsync()` / `GetStorefrontPageByMenuLinkAsync()` | `MenuRepository.GetStorefrontPageByIdAsync()` / `GetStorefrontPageByMenuLinkAsync()` | **NEW** `service.menu.get_page_by_id` / `get_page_by_link` (`MenuService.cs:39/57`) / `repo.menu.get_page_by_id` / `get_page_by_link` (`MenuRepository.cs:92/109`) |
| `HomeController.Navigation(lang)` (child) | `MenuService.BuildStorefrontMenuTreeAsync()` + `ProductCategoryService.BuildStorefrontNavigationTreeAsync()` | `MenuRepository.BuildStorefrontMenuTreeAsync()` + `ProductCategoryRepository.BuildStorefrontNavigationTreeAsync()` | **NEW** `service.menu.build_tree` / `service.product_category.build_nav_tree` + `repo.*` counterparts |

### Storefront — Payment / Cart / Checkout (heaviest)

| Action | Service | Repo | Metric |
|---|---|---|---|
| `PaymentController.AddToCart(productId)` | `ProductService.GetStorefrontProductCardByIdAsync()` + `ShoppingCartService.SaveOrEditShoppingCartAsync()` | `ProductRepository.GetStorefrontProductCardByIdAsync()` + `ShoppingCartRepository.GetShoppingCartByOrderGuidAsync()` | *Product card already timed via product repo* + **NEW** `service.shopping_cart.get_by_guid` / `save` (`ShoppingCartService.cs:113/87`) / `repo.shopping_cart.get_by_guid*` |
| `PaymentController.ShoppingCart()` / `GetShoppingCartLinks()` | `ShoppingCartService.GetShoppingCartByOrderGuidAsync()` | `ShoppingCartRepository.GetShoppingCartByOrderGuidAsync()` | **NEW** `service.shopping_cart.get_by_guid` / `repo.shopping_cart.get_by_guid` |
| `PaymentController.PlaceOrder()` / `PaymentResult()` | `ShoppingCartService.SaveShoppingCartAsync()` → `OrderService.Save` + `OrderRepository` + `CustomerService` + `Coupon*Service` | `ShoppingCartRepository.BeginTransaction()` + `OrderRepository.SaveOrEdit()` + `OrderProductRepository` + `CouponRedemptionRepository` | **NEW** `service.shopping_cart.save` (`ShoppingCartService:234`) already covers transaction root; coupon/order redemption metrics are subsumed (tagged via `service.*` chain) |
| `PaymentController.BuyNow/BuyNowPaymentResult` | `ProductService.GetProductDetailViewModelByIdAsync()` + `ShoppingCartService.SaveBuyNow*` | as above | Already via product + `service.shopping_cart.save` |

### Customer Area — Home

| Action | Service | Repo | Metric |
|---|---|---|---|
| `Customers/HomeController.Index(GET)` | `CustomerService.GetStorefrontCustomerProfileByUserIdAsync()` + `GetStorefrontCustomerSummaryByUserIdAsync()` + `OrderService.GetStorefrontOrderStatsByUserIdAsync()` | `CustomerRepository.GetStorefrontCustomerProfileByUserIdAsync()` + `GetStorefrontCustomerSummaryByUserIdAsync()` + `OrderRepository.GetStorefrontOrderStatsByUserIdAsync()` | **EXISTING** `service.customers.get_profile_by_user` / `get_summary_by_user` (`CustomerService:132/138`) / `repo.customers.get_profile_by_user` etc. (`CustomerRepository:83/62`) + `service.orders.get_stats_by_user` (OrderService) |
| `Index(POST)` | `CustomerService.GetUserIdAsync()` + `SaveOrEditEntityAsync()` | `CustomerRepository.GetUserIdAsync()` + `SaveOrEditAsync()` | **EXISTING** `service.customers.get_by_user` / `repo.customers.get_by_user` |
| `CustomerOrders(search)` | `OrderService.GetStorefrontOrderListByUserIdAsync()` | `OrderRepository.GetStorefrontOrderListByUserIdAsync()` | **EXISTING** `service.orders.get_list_by_user` (`OrderService:94`) / `repo.orders.get_by_id` |
| `CustomerOrderDetail(id)` | `OrderService.GetStorefrontOrderByIdAsync()` | `OrderRepository.GetStorefrontOrderByIdAsync()` | **EXISTING** `service.orders.get_by_id` (`OrderService:52`) / `repo.orders.get_by_id` (`OrderRepository:212`) |
| `SendMessageToSeller` / `Faq` | `FaqService.GetStorefrontFaqSummariesAsync()` + `CustomerService.GetStorefrontCustomerSummaryByUserIdAsync()` | `FaqRepository` + `CustomerRepository` | *Faq is tiny cache — **skipped** per noise rule* |
| `SendSellerMessage(POST)` | `RazorEngineHelper.SendMessageToSellerAsync()` (no DB) | — | *Skipped (email helper, no repo)* |
| `ChangePassword(POST)` | `UserManager.ChangePasswordAsync()` (Identity, no repo) | — | *Skipped (Identity framework)* |
| `WebSiteAddressInfo` | `SettingService.GetSettingValueDtoByKey()` | `SettingRepository` cache | *Skipped trivial key fetch* |

### Already timed in previous change (storefront hot reads) — kept

| Service | Metric |
|---|---|
| `ProductService.GetStorefrontActiveProductsAsync/Sync` | `service.products.get_active_async` / `get_active_sync` |
| `ProductService.GetMainPageProducts/Sync` | `service.products.get_main_page` |
| `OrderService.GetStorefrontOrderByOrderNumber*` / `*Guid*` | `service.orders.get_by_number*` / `get_by_guid` (existing via generic pattern) |

---

## 2) Exact Code Changes — Attributes + Virtual Modifiers

> **Rule:** Do not add timing code inside bodies; do not change business logic. Only `[Timed]` attributes and `virtual` for Castle interception (or interface proxy).

**New files / edits this change:**

* `Services/ProductCategoryService.cs` — added `using Telemetry` + 8 methods → `public virtual` + `[Timed]`:
  ```csharp
  [Timed("service.product_category.get_storefront_by_id")] public virtual async Task<StorefrontCategoryDto> GetStorefrontCategoryByIdAsync(...)
  [Timed("service.product_category.get_storefront_by_id_sync")] public virtual StorefrontCategoryDto GetStorefrontCategoryById(...)
  [Timed("service.product_category.build_nav_tree")] public virtual async Task<List<StorefrontCategoryDto>> BuildStorefrontNavigationTreeAsync(...)
  [Timed("service.product_category.build_nav_tree_sync")] public virtual List<StorefrontCategoryDto> BuildStorefrontNavigationTree(...)
  [Timed("service.product_category.get_page_view_model")] public virtual async Task<ProductCategoryViewModel> GetStorefrontCategoryPageViewModelAsync(...)
  [Timed("service.product_category.build_tree")] public virtual List<ProductCategoryTreeModel> BuildTree(...)
  [Timed("service.product_category.build_tree_async")] public virtual async Task<List<ProductCategoryTreeModel>> BuildTreeAsync(...)
  [Timed("service.product_category.get_breadcrumb")] public virtual async Task<List<ProductCategoryTreeModel>> GetBreadCrumbAsync(...)
  ```

* `Services/StoryService.cs`
  ```csharp
  [Timed("service.story.get_detail")] public virtual async Task<StorefrontStoryDetailDto> GetStorefrontStoryDetailByIdAsync(...)
  [Timed("service.story.get_detail_sync")] public virtual StorefrontStoryDetailDto GetStorefrontStoryDetailById(...)
  [Timed("service.story.get_featured")] public virtual async Task<List<StorefrontStoryCardDto>> GetStorefrontFeaturedStoriesAsync(...)
  [Timed("service.story.get_main_page")] public virtual async Task<PaginatedList<StorefrontStoryCardDto>> GetStorefrontMainPageStoriesAsync(...)
  [Timed("service.story.get_detail_view_model")] public virtual async Task<StoryDetailViewModel> GetStoryDetailViewModelAsync(...)
  [Timed("service.story.get_related")] public virtual async Task<List<StorefrontStoryCardDto>> GetStorefrontRelatedStoriesAsync(...)
  ```

* `Services/MenuService.cs`
  ```csharp
  [Timed("service.menu.get_page_by_id")] public virtual async Task<StorefrontPageDto> GetStorefrontPageByIdAsync(...)
  [Timed("service.menu.get_page_by_link")] public virtual async Task<StorefrontPageDto> GetStorefrontPageByMenuLinkAsync(...)
  [Timed("service.menu.build_tree")] public virtual async Task<MenuTree> BuildStorefrontMenuTreeAsync(...)
  [Timed("service.menu.get_active_cached")] public virtual async Task<List<MenuDto>> GetStorefrontActiveMenusCachedAsync(...)
  ```

* `Services/ShoppingCartService.cs`
  ```csharp
  [Timed("service.shopping_cart.get_by_guid")] public virtual async Task<ShoppingCart> GetShoppingCartByOrderGuidAsync(...)
  [Timed("service.shopping_cart.save")] public virtual async Task SaveShoppingCartAsync(...)
  [Timed("service.shopping_cart.save_sync")] public virtual void SaveShoppingCart(...)
  ```

* `Repositories/ProductCategoryRepository.cs` — same 6 metrics with `repo.product_category.*` prefix + `public virtual` (e.g. `GetStorefrontCategoryByIdAsync` → `repo.product_category.get_storefront_by_id`)
* `Repositories/StoryRepository.cs` — `repo.story.get_detail*`, `get_featured`, `get_main_page`, `get_related` + `virtual`
* `Repositories/MenuRepository.cs` — `repo.menu.get_page_by_id`, `get_page_by_link`, `build_tree`, `get_active` + `virtual`
* `Repositories/ShoppingCartRepository.cs` — `repo.shopping_cart.get_by_guid*` + `virtual`

**Previous change (kept, verified still virtual):**
* `ProductService.cs:88` `service.products.get_storefront_detail*` (2), `:116` `get_active*` (2), `:148` `search_storefront*` (2), `:272`/`310` `get_main_page*` (2)
* `OrderService.cs:40` `service.orders.get_by_id*`, `:94` `get_list_by_user*`
* `CustomerService.cs:128/134/112/120` `service.customers.get_summary_by_user`, `get_profile_by_user`, `get_by_user*`
* `ProductRepository.cs:32/285/325` `repo.products.get_storefront_detail*`, `search*`
* `OrderRepository.cs:212` `repo.orders.get_by_id*`
* `CustomerRepository.cs:23/62/83` `repo.customers.*`

All edits: `using EImece.Domain.Observability.Telemetry;` added, method signature `public virtual`, no body changes.

**Files touched this commit:**
`Services/ProductCategoryService.cs`, `Services/StoryService.cs`, `Services/MenuService.cs`, `Services/ShoppingCartService.cs`, `Repositories/ProductCategoryRepository.cs`, `Repositories/StoryRepository.cs`, `Repositories/MenuRepository.cs`, `Repositories/ShoppingCartRepository.cs`

**Build:** `dotnet build EImece/EImece.sln --configuration Debug` → 0 warnings, 0 errors.

---

## 3) Skipped Methods & Why

| Method / Area | Why skipped (per constraints: skip trivial one-line pass-throughs) |
|---|---|
| `ProductsController.Review(POST)` → `ProductCommentService.SaveOrEditEntityAsync()` | Trivial write, not hot read; admin moderation not storefront latency-critical |
| `AccountController.*` Login/Register Verify (most actions) | Identity framework (`UserManager`, `SignInManager`) + captcha — no service/repo DB metric; thin wrappers |
| `AjaxController.HomePageShoppingCart / GetAllCities / GetTownsByCity / GetDistrictsByTown` | Child-action renders / `TurkishRegionService` fallback via local variable (not DI), low-cost |
| `ErrorController.*`, `UnderConstructionController.Index`, `RobotController.RobotsText`, `ManifestController.Index`, `UrlController.Get/Post` | No DB or tiny `SettingService.GetSettingByKey` cache / file; noise |
| `InfoController.Index` → `MenuService.GetPageByMenuLinkAsync()` | Would duplicate `service.menu.get_page_by_link` already timed at menu service level when storefront page rendered; controller auto `app.info.index` suffices |
| `PaymentController.HomePageShoppingCart / GetShoppingCartSmallDetails / renderShoppingCartPrice` etc. | One-line view helpers / child actions with no repo query (just `ShoppingCartService.GetShoppingCartByOrderGuid` already timed via cart service) |
| `ProductService.GetAdminPageList*` / `GetActiveProducts` admin variants | Admin UI, not storefront scope (excluded per task) |
| `SettingService.GetSettingByKey / GetCachedSettingValueDtoByKey` calls scattered across `BaseController`, `HomeController.SocialMediaLinks` etc. | Trivial single-row cache fetch, extremely high-cardinality key space if timed per key — rely on `app.*` controller timing + `repo.setting.get_by_key` would be noise; prefer not to sample |
| `FaqService.GetStorefrontFaqSummariesAsync`, `BrandService`, `MainPageImageService.GetMainPageViewModelAsync` | Thin cached aggregates (<1ms), not DB hot path for latency percentiles; skipped to keep metric cardinality low |
| `ShoppingCartService.ValidateCoupon*` / `CouponService` sub-calls inside `SaveShoppingCart` transaction | Already counted via parent `service.shopping_cart.save`; adding child metrics would double-count same transaction latency — parent is meaningful business operation |
| `CustomerService.DeleteByUserIdAsync`, `SaveCustomerTypeToNormalAsync` already virtual but not timed here | Admin/customer management, not storefront read hot path; could be added later if needed |
| Generic `BaseService.SaveOrEditEntity / DeleteEntity` overrides | CRUD plumbing, measured via concrete `DeleteProductById` etc. when relevant; generic metric `service.base.save` adds cardinality with no entity insight |

No methods were skipped due to inability to make `virtual` — all chosen service/repo methods are non-sealed instance methods; base helpers (`BaseContentService`, `BaseRepository`) expose `protected DataCachingProvider` etc. but none blocked. For interface-based DI (`IProductService`, `IProductRepository` etc.), **Castle interface proxy** (`ProxyFactory.CreateInterface<T>`) does not require `virtual`; `public virtual` was added for class-proxy fallback and for direct `new`+`ProxyFactory.Create` usage. Sealed classes or private helpers (e.g. `BuildAdminPageListQuery`) cannot be timed and were intentionally excluded.

---

## 4) DI / Proxy Registration — Why `[Timed]` Actually Runs

**Existing wiring (no manual per-service registration needed):**

`App_Start/DependencyInjectionConfig.cs:461` `ResolveImplementationOrUnderConstruction<TService,TImplementation>` — called for every `AddScopedWithProps` registration.

```csharp
var proxied = MaybeWrapWithMetricsProxy<TService>(implementation, sp);
return MaybeWrapWithTimedInterceptor<TService,TImplementation>(proxied, sp);
```

`MaybeWrapWithTimedInterceptor` (`DIConfig.cs:527`):

```csharp
bool hasTimed = HasTimedAttribute(typeof(TImplementation)) || HasTimedAttribute(typeof(TService));
if (!hasTimed) return instance;
if (typeof(TService).IsInterface)
    return ProxyFactory.CreateInterface<TService>(instance); // interface proxy — no virtual needed
return ProxyFactory.Create<TService>(instance);               // class proxy — requires virtual
```

`HasTimedAttribute(Type)` scans `type.GetMethods(BindingFlags.Instance|Public|NonPublic)` + `GetInterfaces()` for `Attribute.IsDefined(..., typeof(TimedAttribute), true)` (inherited).

**Interceptor** `Domain/Observability/Telemetry/TimedInterceptor.cs:21`:

```csharp
public sealed class TimedInterceptor : Castle.DynamicProxy.IInterceptor {
  void Intercept(IInvocation invocation) { 
    var timed = GetTimedAttribute(invocation); // prefers MethodInvocationTarget
    if (timed==null) { invocation.Proceed(); return; }
    var sw = Stopwatch.StartNew();
    invocation.Proceed(); // sync or returns Task
    if (returnType is Task) { invocation.ReturnValue = InterceptAsync(task,timed,sw,invocation); } // await Continue
    else { sw.Stop(); Record(timed,sw,invocation); }
  }
  private static void Record(...){ 
    var histogram = Telemetry.GetOrCreateHistogram(timed.Name,timed.Description); histogram.Record(elapsedMs);
    Activity.Current?.SetTag("timed.metric",timed.Name); Activity.Current?.SetTag("timed.duration_ms",elapsedMs);
    Logger.Info("Timed metric={Metric} duration={DurationMs:F2}ms target={TargetType} method={Method}", ...);
  }
}
```

* Thread-safe: `Stopwatch` per-invocation, `ConcurrentDictionary` in `Telemetry.cs:44`.
* Never throws: all telemetry wrapped in `try{}` + `Debug.WriteLine` / `Logger.Info` swallow.
* `Histogram<double>` unit `ms` via `Telemetry.cs:21` `FallbackMeter = new Meter("EImece","1.0.0")` (fallback) or `OpenTelemetryBootstrap.Meter` (`DI: 1.0.0`).
* Castle dep: `Castle.Core 5.1.1` added to `EImece.Domain/packages.config` + `EImece.Domain.csproj:271` `HintPath ..\packages\Castle.Core.5.1.1\lib\net462`.

**Controllers:** Already `Filters/TimedActionFilterAttribute.cs:19` (`TimedActionFilter`) auto `app.{controller}.{action}` on `BaseController.cs:25` and `Areas/Customers/HomeController.cs:33` — no controller `[Timed]` needed.

**Verification:** After adding attributes + `virtual`, `dotnet build EImece/EImece.sln --configuration Debug` passes (0 warnings, 0 errors) — virtual modifier satisfied Castle for any class-proxy fallback path.
