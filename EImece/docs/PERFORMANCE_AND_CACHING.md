# Database Performance, Read Models & Hierarchical Caching (ASP.NET MVC 5 + EF6)

This document describes the architectural pattern in EImece for high-throughput storefront reads, light projection DTOs, strict separation from admin panel EF change-tracking, hierarchical cache taxonomy, and precise cache invalidation.

---

## 1. Architectural Boundary: Storefront Read Models vs. Admin Panel

| Dimension | Storefront Read Paths | Admin Panel Operations |
|---|---|---|
| **Data Shape** | Lightweight DTOs (`StorefrontProductCardDto`, `StorefrontCategoryDto`, `StorefrontMenuDto`, `StorefrontStoryCardDto`, `StorefrontBrandDto`, `StorefrontTagDto`, `StorefrontBannerDto`, `FaqDto`, `OrderDto`) | Full EF Entities (`Product`, `ProductCategory`, `Menu`, `Story`, `Brand`, `Tag`, `MainPageImage`, `Faq`, `Order`, `Customer`) |
| **Tracking Mode** | Strict `AsNoTracking()` | EF Change Tracker active (`BaseEntityService`, `EntityRepository`) |
| **Query Strategy** | Direct LINQ `.Select(dto => ...)` projection at repository boundary — zero entity allocations, no navigation graph loads | Repository `GetAdminPageList`, `GetSingle`, `GetAll`, `SaveOrEditEntity`, `Delete...` |
| **Caching Target** | Cached directly in memory via `DataCachingProvider.GetOrAddAsync(key, factory, ttl)` | Not cached in read DTO cache; always fresh from DB |
| **View Models** | Pure FrontModels (`ProductCategoryViewModel`, `ProductDetailViewModel`, `StoryDetailViewModel`, `StoryCategoryViewModel`, etc.) containing only DTOs and value types | Entity-backed admin models and forms |

---

## 2. Storefront Aggregates & Projections

All storefront-heavy aggregates project directly from the database into dedicated DTOs without loading full entity graphs:

### 1. Product (List Cards, Detail, Related, Search Results)
- **DTOs:** `StorefrontProductCardDto`, `StorefrontProductDetailDto`
- **Repositories:** `ProductRepository.GetStorefrontProductsByCategoryIdAsync`, `GetStorefrontProductDetailByIdAsync`, `GetStorefrontRelatedProductsAsync`, `GetStorefrontRandomProductsByCategoryId`
- **Rule:** Computes `PriceWithDiscount`, discount percentages, SEO URLs, and cropped image endpoints cleanly on the DTO.

### 2. Category (Navigation, Browsing, Breadcrumbs)
- **DTOs:** `StorefrontCategoryDto`, `StorefrontCategoryTreeDto`
- **Repositories:** `ProductCategoryRepository.GetStorefrontCategoriesAsync`, `GetStorefrontCategoryByIdAsync`, `GetStorefrontActiveCategoriesAsync`
- **Features:** Self-contained parent hierarchy, theme metadata, and child category lists.

### 3. Brand & Tag
- **DTOs:** `StorefrontBrandDto`, `StorefrontTagDto`
- **Repositories/Services:** `BrandService.GetStorefrontBrandsAsync`, `TagService.GetStorefrontProductTagsAsync`, `GetStorefrontTagsWithStoryCountsAsync`, `GetStorefrontTagsWithEntityCountsAsync`
- **Features:** Entity counts (products, stories) calculated in database projection.

### 4. Menu, Banner, Story, FAQ
- **DTOs:** `StorefrontMenuDto`, `StorefrontBannerDto`, `StorefrontStoryCardDto`, `StorefrontStoryDetailDto`, `FaqDto`
- **Services:** `MenuService.BuildStorefrontMenuTreeAsync`, `MainPageImageService.GetStorefrontMainPageBannersAsync`, `StoryService.GetStorefrontStoriesByCategoryIdAsync`, `FaqService.GetStorefrontFaqsAsync`

### 5. Cart Summary & Mini-Cart
- **DTOs / Models:** `ShoppingCartProduct` (instantiable directly from `StorefrontProductDetailDto` or `StorefrontProductCardDto`), `ShoppingCartSession`
- **Rule:** Mini-cart views and session objects consume lightweight models without attaching EF entities to session state.

### 6. Order Tracking & Customer Account Views
- **DTOs:** `OrderDto`, `OrderProductDto`, `CustomerDto`
- **Repositories/Services:** `OrderRepository.GetStorefrontOrderByIdAsync`, `GetStorefrontOrderByOrderNumberAsync`, `GetStorefrontOrderByGuidAsync`, `GetStorefrontOrdersByUserIdAsync`
- **ViewModels:** `CustomerOrderDetailViewModel`, `CustomerOrdersViewModel`

---

## 3. Hierarchical Cache Key Strategy (`CacheKeys.cs`)

Hierarchical keys use structured, colon-delimited prefixes: `{area}:{family}:{variant}:{dimensions...}`.

```csharp
// Prefix constants for atomic subtree invalidation
CacheKeys.ProductListPrefix       // "product:list:"
CacheKeys.ProductDetailPrefix     // "product:detail:"
CacheKeys.ProductSearchPrefix     // "product:search:"
CacheKeys.CategoryPrefix          // "category:"
CacheKeys.BrandPrefix             // "brand:"
CacheKeys.TagPrefix               // "tag:"
CacheKeys.MenuPrefix              // "menu:"
CacheKeys.BannerPrefix            // "banner:"
CacheKeys.StoryPrefix             // "story:"
CacheKeys.FaqPrefix               // "faq:"
CacheKeys.OrderPrefix             // "order:"
CacheKeys.SettingPrefix           // "setting:"

// Concrete key generators
CacheKeys.MainPageProducts(page, lang)                        // "product:list:mainpage:p{page}:lang{lang}"
CacheKeys.CategoryProducts(catId, lang, page, sort, ...)      // "product:list:cat:{catId}:p{page}:s{sort}:lang{lang}..."
CacheKeys.ProductDetail(productId, lang)                      // "product:detail:{productId}:lang{lang}"
CacheKeys.CategoryTree(lang)                                  // "category:tree:lang{lang}"
CacheKeys.MenuTree(lang)                                      // "menu:tree:lang{lang}"
CacheKeys.MainPageBanners(lang)                               // "banner:mainpage:lang{lang}"
CacheKeys.StoryCategories(lang)                               // "story:category:active:lang{lang}"
CacheKeys.FaqList(lang)                                       // "faq:list:lang{lang}"
CacheKeys.StorefrontOrder(orderId)                            // "order:storefront:id:{orderId}"
```

---

## 4. Cache TTL Policy

| Aggregate / Query | TTL Policy | Config Setting | Default Duration | Rationale |
|---|---|---|---|---|
| **Product Detail / Category Browse** | Absolute | `AppConfig.CacheMediumSeconds` | 300 seconds (5 min) | Stable browsing data; bounded freshness |
| **Product Search Results** | Absolute | `AppConfig.CacheShortSeconds` | 10 seconds | High parameter cardinality; keeps memory bounded |
| **Menu Tree & Navigation** | Absolute / Sliding | `AppConfig.CacheLongSeconds` | 1800 seconds (30 min) | High read traffic, low mutation rate |
| **Main Page Banners / Sliders** | Absolute | `AppConfig.CacheLongSeconds` | 1800 seconds (30 min) | Seldom mutated marketing material |
| **Story Categories & Blog Index** | Absolute | `AppConfig.CacheMediumSeconds` | 300 seconds (5 min) | Editorial content |
| **FAQs** | Absolute | `AppConfig.CacheLongSeconds` | 1800 seconds (30 min) | Infrequent changes |
| **Brand & Tag Directory** | Absolute | `AppConfig.CacheLongSeconds` | 1800 seconds (30 min) | Catalog taxonomies |
| **Customer Orders** | No Cache / Keyed Lookup | N/A | Fresh per request | PII / User transactional state |

---

## 5. Invalidation Trigger Points

Admin actions trigger clean, prefix-based cache purges via `DataCachingProvider.ClearByPrefix(...)`:

| Admin Action / Invalidation Event | Service / Method | Invalidation Trigger |
|---|---|---|
| **Product Save / Edit / Activation** | `ProductService.SaveOrEditEntityAsync` | `ClearByPrefix(CacheKeys.ProductListPrefix)`<br>`ClearByPrefix(CacheKeys.ProductDetailPrefix)`<br>`ClearByPrefix(CacheKeys.ProductSearchPrefix)` |
| **Product Delete** | `ProductService.DeleteProductByIdAsync` | Same as product save |
| **Category Save / Move / Order** | `ProductCategoryService.SaveOrEditEntityAsync` | `ClearByPrefix(CacheKeys.CategoryPrefix)`<br>`ClearByPrefix(CacheKeys.ProductListPrefix)` |
| **Brand Save / Delete** | `BrandService.SaveOrEditEntityAsync` | `ClearByPrefix(CacheKeys.BrandPrefix)`<br>`ClearByPrefix(CacheKeys.ProductListPrefix)` |
| **Tag Save / Delete** | `TagService.SaveOrEditEntityAsync` | `ClearByPrefix(CacheKeys.TagPrefix)`<br>`ClearByPrefix(CacheKeys.ProductListPrefix)` |
| **Menu Tree Edit / Item Move** | `MenuService.SaveOrEditEntityAsync` / `MoveMenu` | `ClearByPrefix(CacheKeys.MenuPrefix)` |
| **Banner Add / Reorder / Delete** | `MainPageImageService.SaveOrEditEntityAsync` | `ClearByPrefix(CacheKeys.BannerPrefix)` |
| **Story / Blog Post Save / Delete** | `StoryService.SaveOrEditEntityAsync` | `ClearByPrefix(CacheKeys.StoryPrefix)` |
| **Story Category Save / Delete** | `StoryCategoryService.SaveOrEditEntityAsync` | `ClearByPrefix(CacheKeys.StoryCategoryPrefix)`<br>`ClearByPrefix(CacheKeys.StoryPrefix)` |
| **FAQ Save / Delete** | `FaqService.SaveOrEditEntityAsync` | `ClearByPrefix(CacheKeys.FaqPrefix)` |
| **Full Site Refresh Button** | `DashboardController.ClearCache` | `ClearAll()` drops in-memory cache, OutputCache, and schedules background warm-up |
