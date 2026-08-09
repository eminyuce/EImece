# Database performance & MemoryCache (ASP.NET MVC 5 + EF6)

This guide shows how EImece reduces SQL load and raises concurrency on a **single-process**
ASP.NET MVC 5 host: eager-loaded async EF6 queries, covering SQL Server indexes, and
`MemoryCache` / LazyCache with hierarchical keys, absolute expiration, and prefix invalidation.

Related: [ASYNC_AWAIT_GUIDE.md](./ASYNC_AWAIT_GUIDE.md) (thread-pool benefits of `ToListAsync`).

---

## 1. Optimize EF6 queries (eager load, AsNoTracking, async)

### The N+1 anti-pattern (before)

```csharp
// BAD — one query for products, then one query per product for Category / MainImage
public ActionResult Index(int page = 1)
{
    var products = db.Products
        .Where(p => p.IsActive && p.Lang == lang)
        .OrderByDescending(p => p.Position)
        .Skip((page - 1) * pageSize).Take(pageSize)
        .ToList(); // blocks a thread-pool thread for the whole round-trip

    foreach (var p in products)
    {
        // LazyLoadingEnabled is false in EImeceContext, but the same N+1 appears
        // if the view touches navigation properties that were never Include()'d —
        // either as NullReferenceExceptions or as extra explicit loads.
        ViewBag.CategoryName = p.ProductCategory.Name;
    }
    return View(products);
}
```

Under concurrency that pattern multiplies SQL round-trips by page size and holds ASP.NET
threads while they wait on I/O.

### Rewritten ProductsController + repository path (after)

Controller — already async; releases the request thread while SQL runs:

```csharp
[CustomOutputCache(CacheProfile = Constants.Cache20Minutes)]
public async Task<ActionResult> Index(CancellationToken cancellationToken, int page = 1)
{
    var products = await ProductService.GetMainPageProductsAsync(page, CurrentLanguage, cancellationToken);
    return View(products);
}
```

Repository — one SQL batch with `Include` (eager load) + `AsNoTracking` + `ToListAsync`:

```csharp
// ProductRepository.GetActiveProductsAsync
Expression<Func<Product, object>>[] includes = {
    r => r.ProductFiles,
    r => r.ProductCategory,
    r => r.ProductTags.Select(t => t.Tag),
    r => r.MainImage
};
Expression<Func<Product, bool>> match = r => r.IsActive && r.Lang == language;

// PaginateDescendingAsync → GetAllIncludingReadOnly → AsNoTracking + Include + OrderBy
// then CountAsync + Skip/Take + ToListAsync (see QueryableExtensions / EntityRepository)
return await PaginateDescendingAsync(pageIndex, pageSize, t => t.Position, match, cancellationToken, includes);
```

Search path trims the term **once in CLR** so EF does not emit per-predicate `LTRIM/RTRIM`,
and still eager-loads category / image / tags:

```csharp
var term = (search ?? string.Empty).Trim();
Expression<Func<Product, bool>> match = r =>
    r.IsActive && r.Lang == lang
    && (r.Name.Contains(term) || r.NameLong.Contains(term) || r.NameShort.Contains(term));
```

### Why this scales better (single process)

| Technique | Effect under concurrency |
|---|---|
| `Include` / eager load | Caps round-trips at ~1–2 per page instead of 1 + N |
| `AsNoTracking` (`FindAllIncludingReadOnly`, `GetAllIncludingReadOnly`) | Skips change-tracker allocations on read-only graphs |
| `async` + `ToListAsync` | Returns the ASP.NET thread to the pool during SQL wait |
| Covering indexes (below) | Turns scans into seeks so each await finishes sooner |

---

## 2. SQL Server indexes & execution plans

Apply:

```text
EImece/SqlScripts/AddPerformanceIndexes.sql
```

Key indexes (product tables first):

| Index | Supports |
|---|---|
| `IX_Products_ProductCategoryId_IsActive_Lang_Position` | **Category browse / counts / related-by-category** |
| `IX_Products_ProductCategoryId_Lang` | Admin category grids |
| `IX_Products_IsActive_Lang_Position` | Storefront listing / paging |
| `IX_Products_IsActive_MainPage_Lang_Position` | Home main-page products |
| `IX_Products_BrandId_Lang` / `ProductCode` / `State` / `IsCampaign` | Brand + admin filters |
| `IX_Products_MainImageId` | MainImage FK joins |
| `IX_ProductCategories_ParentId_IsActive_Position` | Category tree |
| `IX_ProductCategories_Lang_IsActive_Position` / `MainPage_…` | Navigation / home categories |
| `IX_ProductTags_ProductId_TagId` / `TagId_ProductId` | Detail tags + related-by-tag |
| `IX_ProductFiles_ProductId` / `FileStorageId` | Gallery eager loads |
| `IX_ProductComments_ProductId_Lang_IsActive` | Detail reviews |
| `IX_ProductSpecifications_ProductId` | Detail specs |
| `IX_Brands_Lang_IsActive_Position` | Brand lists |
| `IX_Orders_OrderNumber` / `UserId_UpdatedDate` | Order lookups |
| `IX_OrderProducts_ProductId` / `_OrderId` | Eager order lines + delete guards |

### How to monitor plans

Use `EImece/SqlScripts/MonitorQueryExecutionPlans.sql`:

1. **SSMS actual plan** — `Ctrl+M`, run the sample listing/`OrderNumber` queries. Prefer
   *Index Seek* (or ordered covering scan) over *Clustered Index Scan* + *Key Lookup*.
2. **`SET STATISTICS IO/TIME`** — watch logical reads drop after the covering indexes land.
3. **`sys.dm_exec_query_stats`** — find hot `Products` / `Orders` statements by
   `total_logical_reads`.
4. **`sys.dm_db_missing_index_*`** — heuristics only; validate with actual plans before creating.
5. **`sys.dm_db_index_usage_stats`** — confirm new indexes show `user_seeks`, not only `user_updates`.

EF tip: enable `EfSqlLogger` in staging, copy the SQL, paste into SSMS with an actual plan.
Do not leave Extended Events `query_post_execution_showplan` on in production.

---

## 3. MemoryCache strategy (absolute expiry, invalidation, key structure)

### Hierarchical keys (`CacheKeys`)

```csharp
// Logical keys (providers add a physical "Memory:" prefix themselves)
CacheKeys.MainPageProducts(page, language);     // product:list:mainpage:p1:lang1
CacheKeys.ActiveProducts(language);             // product:list:active:lang1
CacheKeys.ProductSearch(q, page, size, lang, sort); // product:search:qshoes:p1:...
```

Shape: `{area}:{family}:{variant}:{dimensions…}`

- **Collision avoidance** — product lists never share a key with settings/menus.
- **Prefix invalidation** — `ClearByPrefix(CacheKeys.ProductListPrefix)` drops every list
  entry (sync + async) without `ClearAll()`.

### Absolute expiration for product lists

```csharp
return DataCachingProvider.GetOrAdd(
    CacheKeys.ActiveProducts(language),
    () => ProductRepository.GetActiveProducts(language),
    CachePolicy.Absolute(AppConfig.CacheMediumSeconds)); // default 300s
```

Search uses a **short** absolute TTL (`CacheShortSeconds`, default 10s) because key cardinality
is high:

```csharp
DataCachingProvider.GetOrAdd(
    CacheKeys.ProductSearch(search, pageIndex, pageSize, lang, sorting),
    () => BuildProductsSearchViewModel(...),
    CachePolicy.Absolute(AppConfig.CacheShortSeconds));
```

### Invalidate when data changes

```csharp
public override Product SaveOrEditEntity(Product entity)
{
    var saved = base.SaveOrEditEntity(entity);
    InvalidateProductListCaches(); // ClearByPrefix list + search
    return saved;
}
```

`DeleteProductById` calls the same invalidation after a successful delete. In a single-process
app this is enough: the next request rebuilds from SQL via single-flight `GetOrAdd`.

### Sliding expiration API

```csharp
// Good for rarely changing, continuously read data (settings, address trees)
DataCachingProvider.GetOrAdd(key, factory, CachePolicy.Sliding(AppConfig.CacheLongSeconds));
```

Both `MemoryCacheProvider` and `LazyCacheProvider` honour `CachePolicy`.

---

## 4. Balancing DB load vs cache freshness

| Scenario | Prefer | Why |
|---|---|---|
| Product catalogues, search pages, prices | **Absolute** | Guarantees a freshness bound even when the key stays hot |
| After admin save/delete | **Invalidate + Absolute** | Immediate correctness; absolute is a safety net if a prefix is missed |
| Settings, menus, region trees | **Sliding** (or long Absolute) | Low mutation rate; traffic keeps the entry warm and off SQL |
| Per-user / high-cardinality keys | Short Absolute or **no cache** | Avoid MemoryCache bloat in one worker process |
| Async factories shared across requests | Absolute + `CancellationToken.None` in factory | Do not bind a shared cache entry to one request's token |

### Single-process caveats

- `MemoryCache` / LazyCache are **in-process**. Multiple IIS workers each have their own copy;
  invalidation does not cross processes. Prefer one worker or accept brief divergence until Absolute TTL.
- Single-flight (`GetOrAdd` / `Lazy<T>`) stops stampedes on expiry — critical when Absolute TTLs
  align under load.
- Output cache (`CustomOutputCache`) and data cache are complementary: output cache skips MVC
  entirely; data cache protects shared service/repository work when output cache misses.

### Config knobs (`Web.config` / `AppConfig`)

| Key | Default | Typical use |
|---|---|---|
| `CacheShortSeconds` | 10 | Search results |
| `CacheMediumSeconds` | 300 | Product lists |
| `CacheLongSeconds` | 1800 | Menus, tags, settings |
| `IsCacheActive` | true | Global kill switch |

---

## Files touched by this work

| Area | Path |
|---|---|
| Cache keys / policy | `EImece.Domain/Caching/CacheKeys.cs`, `CachePolicy.cs`, `CacheExpirationMode.cs` |
| Providers | `MemoryCacheProvider.cs`, `LazyCacheProvider.cs`, `IEimeceCacheProvider.cs` |
| Product queries | `Repositories/ProductRepository.cs`, `GenericRepository.EntityFramework/EntityRepository'2.cs` |
| Product cache + invalidation | `Services/ProductService.cs` |
| Order lookups | `Repositories/OrderRepository.cs` |
| SQL | `EImece/SqlScripts/AddPerformanceIndexes.sql`, `MonitorQueryExecutionPlans.sql` |
