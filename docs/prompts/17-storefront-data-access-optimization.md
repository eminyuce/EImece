# Storefront service/repository data-access optimization

- **Captured:** 2026-08-14 8:47:34 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

# EImece — Full Storefront Service/Repository Data-Access Optimization
Storefront and admin methods should be kept separate, with clear, purpose-specific naming.
We need to optimize the ENTIRE STOREFRONT / END-USER data-access layer of the EImece application.

This is a performance-focused refactoring of the existing:

Controller
    ↓
Storefront Service
    ↓
Repository
    ↓
Entity Framework 6
    ↓
SQL Server

Do NOT limit this work to ProductCategory/Product.

The storefront uses many different tables and domains, including:

## Catalog & Merchandising
- Products
- ProductCategories
- Categories
- Brands
- Tags
- Galleries
- Product images
- FileStorage
- ProductFiles
- ProductTags
- Filters
- Filter values
- Product specifications
- Product sorting
- Configurable sorting
- Related products
- Other catalog/merchandising entities discovered in the codebase

## Content
- Menus
- Menu items
- Banner carousels
- Banners
- Stories
- Blog/content
- Themed pages
- Page templates
- Page sections
- Content blocks
- SEO/content data
- Mail templates where relevant to storefront
- Other CMS/content entities discovered in the codebase

Also inspect all OTHER entities used by the storefront. Do not assume the above list is complete.

--------------------------------------------------
# TECHNOLOGY / ARCHITECTURE
--------------------------------------------------

- ASP.NET MVC 5.3
- .NET Framework 4.8.1
- Entity Framework 6.5
- SQL Server
- ASP.NET Identity / OWIN
- Repository + Service layer architecture
- Razor Views
- Existing application architecture must be preserved

DO NOT:

- migrate to EF Core
- upgrade frameworks
- replace the Repository/Service architecture
- introduce an unrelated ORM
- rewrite the application
- perform unrelated cleanup
- change business behavior without evidence

This is an optimization/refactoring project, not a rewrite.

--------------------------------------------------
# PRIMARY OBJECTIVE
--------------------------------------------------

The current application sometimes loads complete EF entities and large navigation graphs even though storefront pages only need a small subset of the data.

The goal is to change the storefront READ path toward:

HTTP Request
    ↓
Controller
    ↓
Storefront Service
    ↓
Storefront Repository
    ↓
EF6 LINQ Projection
    ↓
SQL Server
    ↓
Only required rows + columns
    ↓
DTO / Read Model
    ↓
ViewModel if required
    ↓
Razor View

Instead of:

HTTP Request
    ↓
Controller
    ↓
Service
    ↓
Repository
    ↓
Full EF Entity
    ↓
Many Includes
    ↓
Large entity graph
    ↓
SQL Server
    ↓
Unnecessary rows/columns
    ↓
EF materialization
    ↓
Razor View

Primary performance objectives:

1. Reduce SQL Server work.
2. Reduce rows returned.
3. Reduce columns returned.
4. Reduce EF materialization.
5. Reduce memory consumption.
6. Reduce change-tracking overhead.
7. Prevent unnecessary navigation loading.
8. Prevent N+1 queries.
9. Prevent accidental lazy loading from Razor views.
10. Apply filtering in SQL rather than C#.
11. Apply sorting in SQL rather than C#.
12. Apply pagination in SQL rather than C#.
13. Use appropriate DTOs/read models.
14. Use AsNoTracking() for read-only storefront queries.
15. Preserve all existing storefront behavior.
16. Preserve admin behavior.

--------------------------------------------------
# VERY IMPORTANT: FIRST AUDIT, THEN CODE
--------------------------------------------------

DO NOT immediately start modifying code.

First inspect the entire solution and create an internal map of:

Entity
→ Repository
→ Service
→ Controller
→ Action
→ View
→ Partial View
→ Layout / Child Action
→ Actual properties used

Search the entire repository.

Find all storefront entry points.

At minimum inspect:

- Homepage
- Category pages
- Product listing
- Product detail
- Search
- Filters
- Sorting
- Brand pages
- Tag pages
- Gallery pages
- Menu/navigation
- Header
- Footer
- Banner/carousel
- Stories
- Blog
- Themed pages
- CMS content
- Campaign/landing pages
- AJAX storefront endpoints
- Cart-related reads
- Wishlist-related reads
- Customer-facing pages
- Any other end-user functionality

Do NOT assume a method is storefront-only based on its name.

Trace every caller.

--------------------------------------------------
# CRITICAL BUSINESS RULE:
# MAIN ENTITY ACTIVATION VS RELATIONSHIP ENTITIES
--------------------------------------------------

This is one of the most important requirements.

DO NOT treat relationship/junction entities as the entities that control storefront visibility.

Activation/visibility normally belongs to the MAIN BUSINESS ENTITY.

Examples of main/domain entities:

- Product
- ProductCategory
- Brand
- Tag
- FileStorage
- Menu
- Banner
- Story
- Blog
- Page
- Gallery
- Filter
- FilterValue
- etc.

Relationship entities can include:

- ProductTag
- ProductFile
- ProductCategoryProduct
- GalleryItem
- other junction/association entities

The relationship entity's IsActive MUST NOT automatically be used for storefront filtering.

Inspect the actual domain model and existing business logic before deciding.

--------------------------------------------------
# PRODUCT EXAMPLE — REQUIRED BEHAVIOR
--------------------------------------------------

For a storefront category/product query, the desired graph is:

Category
└── Active Products
    ├── MainImage
    ├── ProductFiles
    │   └── Active FileStorage
    └── ProductTags
        └── Active Tag

Meaning:

Product.IsActive
    → IMPORTANT

ProductFile.IsActive
    → NOT the visibility rule

FileStorage.IsActive
    → IMPORTANT

ProductTag.IsActive
    → NOT the visibility rule

Tag.IsActive
    → IMPORTANT

For example:

Product #100
    IsActive = true

ProductFiles:
    ProductFile #1 → FileStorage #10 → IsActive = true
    ProductFile #2 → FileStorage #11 → IsActive = false
    ProductFile #3 → FileStorage #12 → IsActive = true

The storefront should return:

FileStorage #10
FileStorage #12

and NOT:

FileStorage #11

The ProductFile relationship itself does not need an IsActive condition.

Likewise:

ProductTags:
    ProductTag #1 → Tag #10 → IsActive = true
    ProductTag #2 → Tag #11 → IsActive = false
    ProductTag #3 → Tag #12 → IsActive = true

The storefront should return:

Tag #10
Tag #12

and NOT:

Tag #11

Do NOT add:

ProductFile.IsActive

or:

ProductTag.IsActive

unless the existing application explicitly demonstrates that those properties are intended to control storefront visibility.

--------------------------------------------------
# GENERAL ACTIVATION RULE
--------------------------------------------------

For every entity that has:

IsActive
Published
Visible
State
LinkIsActive
StartDate
EndDate
or another visibility-related property:

DO NOT automatically add a WHERE condition.

First determine:

1. Is this a main business entity?
2. Is it independently managed?
3. Can an administrator activate/deactivate it?
4. Does that activation control storefront visibility?
5. Is the property actually used elsewhere as a business rule?

Only then apply the appropriate condition.

Never invent activation semantics.

--------------------------------------------------
# EF6 FILTERED INCLUDE
--------------------------------------------------

EF6 does NOT support modern filtered Include().

Do NOT attempt:

.Include(x => x.Products.Where(...))

Do NOT attempt to solve this optimization by putting Where() inside Include().

For storefront queries requiring filtered child collections, use:

LINQ projection:

.Select(...)

For example:

Products
    .Where(p => p.IsActive)
    .Select(...)

and:

ProductFiles
    .Where(pf => pf.FileStorage.IsActive)
    .Select(...)

and:

ProductTags
    .Where(pt => pt.Tag.IsActive)
    .Select(...)

--------------------------------------------------
# PROJECTION
--------------------------------------------------

For read-only storefront queries, prefer:

.AsNoTracking()

and:

.Select(x => new SomeStorefrontDto
{
    ...
})

Only retrieve the fields required by the consuming page/component.

Do NOT load an entire EF entity when the page only needs a few properties.

Bad:

var product = await repository.GetProductWithEverythingAsync(...);

Good:

var product = await repository.GetProductForStorefrontAsync(...);

where the repository projects directly into the required read model.

--------------------------------------------------
# DTO STRATEGY
--------------------------------------------------

DO NOT convert every EF entity into a DTO.

A DTO is justified when it provides meaningful value such as:

- fewer selected columns
- fewer joins
- reduced entity materialization
- no tracking
- protection against lazy loading
- explicit storefront contract
- reusable read model
- reduced memory usage

Do NOT create:

Entity
→ DTO with exactly the same 50 properties
→ ViewModel with exactly the same 50 properties

That does not provide meaningful optimization.

Instead create purpose-specific read models.

Examples:

StorefrontProductDto
StorefrontProductCardDto
StorefrontProductDetailDto
StorefrontCategoryDto
StorefrontBrandDto
StorefrontTagDto
StorefrontGalleryDto
StorefrontMenuDto
StorefrontBannerDto
StorefrontStoryDto
StorefrontPageDto
StorefrontFilterDto

Use different DTOs when different pages need materially different data.

For example:

Product card:

Id
Name
Price
Discount
MainImage

Product detail:

Id
Name
Description
Price
MainImage
Gallery
Tags
Specifications
Brand
Category
etc.

Do not force both to use one giant Product DTO.

--------------------------------------------------
# VIEWMODEL STRATEGY
--------------------------------------------------

DTOs should represent optimized data retrieval/read models.

ViewModels should represent page/UI requirements.

For example:

CategoryPageViewModel

may contain:

Category
Products
Filters
SortingOptions
Pagination

while:

ProductStorefrontDto

contains only product data retrieved from SQL.

Do not expose EF entities directly to storefront Razor views unless there is a strong existing reason.

--------------------------------------------------
# AUDIT RAZOR VIEWS
--------------------------------------------------

For every major storefront view:

1. Inspect the main .cshtml.
2. Inspect all partials.
3. Inspect nested partials.
4. Inspect layout dependencies.
5. Inspect child actions.
6. Inspect helper methods.
7. Inspect JavaScript/AJAX data requirements.
8. Identify every Model property accessed.
9. Identify navigation properties accessed.
10. Identify properties that cause lazy loading.

Example:

If a product card only uses:

product.Id
product.Name
product.Price
product.MainImage.Url

do NOT load:

ProductFiles
ProductTags
Tags
Specifications
Reviews
Brand
Category
Gallery
other navigation properties

unless actually required.

--------------------------------------------------
# FULL STOREFRONT DOMAIN AUDIT
--------------------------------------------------

For every major storefront domain determine:

1. What data does the page actually require?
2. Which columns are actually required?
3. Which relationships are actually required?
4. Which main entities must be active?
5. Which relationships are merely associations?
6. Which child entities have independent activation?
7. Can projection be used?
8. Can AsNoTracking() be used?
9. Is pagination needed?
10. Is sorting happening in SQL?
11. Is filtering happening in SQL?
12. Is lazy loading occurring?
13. Is N+1 occurring?
14. Can caching be used?
15. Are appropriate indexes available?

--------------------------------------------------
# CATALOG / PRODUCT OPTIMIZATION
--------------------------------------------------

Audit:

Products
Categories
ProductCategories
Brands
Tags
FileStorage
Galleries
ProductFiles
ProductTags
Specifications
Filters
Filter values
Sorting
Related products
and all other discovered catalog entities.

For each storefront query:

- filter active main entities in SQL
- project only required columns
- use AsNoTracking()
- avoid unnecessary Include()
- avoid loading unused navigation properties
- avoid N+1
- paginate large lists
- sort in SQL
- filter in SQL

--------------------------------------------------
# CONTENT OPTIMIZATION
--------------------------------------------------

Audit:

Menus
Menu items
Banners
Banner carousels
Stories
Blog
Themed pages
Page templates
Page sections
Content blocks
SEO metadata
other CMS entities

Do not retrieve complete content when a listing only needs:

Id
Title
Slug
Summary
Thumbnail

For example, a blog listing should not retrieve a huge HTML Content column unless it is actually displayed.

For content detail pages, retrieve the full content only when required.

--------------------------------------------------
# MENU OPTIMIZATION
--------------------------------------------------

Audit menu queries.

Determine:

- active menu rules
- language
- hierarchy
- parent/child relationships
- position
- link visibility
- page/theme requirements
- images

Do not load all menu records and filter in C#.

Use SQL-side filtering and projection.

Do not introduce activation checks on relationship records unless the existing business logic explicitly requires them.

--------------------------------------------------
# BANNER / CAROUSEL OPTIMIZATION
--------------------------------------------------

Audit:

- active state
- date range
- language
- placement
- position
- image
- mobile/desktop behavior
- content

Retrieve only banners required by the current page and placement.

Do not retrieve every banner and filter later in memory.

--------------------------------------------------
# FILTER OPTIMIZATION
--------------------------------------------------

Audit storefront product filters.

Filtering must occur in SQL.

Do NOT:

load thousands of products
→ ToList()
→ filter in C#

Prefer:

query
→ Where()
→ Select()
→ Skip()
→ Take()
→ SQL Server

Apply filters before materialization.

--------------------------------------------------
# SORTING OPTIMIZATION
--------------------------------------------------

Audit all configurable product sorting.

Determine whether sorting currently occurs:

1. in SQL
or
2. in application memory.

Prefer SQL Server sorting.

Avoid:

var products = await query.ToListAsync();

products = products.OrderBy(...);

when the ordering can be translated to SQL.

Inspect all sorting modes.

Ensure ordering is deterministic when pagination is used.

--------------------------------------------------
# PAGINATION
--------------------------------------------------

Never load an entire large product/search/story dataset when the page only displays a limited number.

Use:

.Skip(...)
.Take(...)

or a more appropriate keyset strategy where justified.

Pagination must occur before materialization.

--------------------------------------------------
# N+1 QUERY AUDIT
--------------------------------------------------

Search for N+1 patterns in:

- Controllers
- Services
- Repositories
- Razor views
- partials
- helpers
- child actions

Example problem:

foreach (var product in products)
{
    product.Brand.Name;
}

if Brand is lazily loaded for every product.

Do not solve every N+1 problem by blindly adding Include().

Use a properly shaped projection where appropriate.

--------------------------------------------------
# LAZY LOADING
--------------------------------------------------

Identify storefront code where Razor views trigger lazy loading.

Example:

@Model.Product.Brand.Name
@Model.Product.MainImage.Url
@Model.Product.Category.Name

If those relationships were not deliberately loaded/projected, this may generate unexpected SQL.

The optimized storefront ViewModel/DTO should contain what the view needs.

The Razor view should not be responsible for database loading.

--------------------------------------------------
# CHILD ACTION / ASP.NET MVC 5 WARNING
--------------------------------------------------

This application uses ASP.NET MVC 5.

Do NOT convert MVC child actions into async actions merely for this optimization.

Be extremely careful with:

Html.Action(...)
Html.RenderAction(...)

and layout/partial dependencies.

ASP.NET MVC 5 has limitations around asynchronous child actions.

Do not introduce:

HttpServerUtility.Execute blocked while waiting for asynchronous operation

or similar regressions.

Keep child-action behavior compatible with MVC 5.

--------------------------------------------------
# REPOSITORY LAYER
--------------------------------------------------

Inspect the current generic repository.

If the repository relies heavily on methods such as:

GetSingleIncludingAsync()
GetAllIncludingAsync()
GetIncludePropertyExpressionList()

do NOT force optimized storefront queries through these methods if they cause over-fetching.

Introduce specialized storefront read methods where appropriate.

Examples:

GetProductForStorefrontAsync()
GetProductCategoryForStorefrontAsync()
GetProductsForStorefrontAsync()
GetBrandForStorefrontAsync()
GetTagsForStorefrontAsync()
GetMenuForStorefrontAsync()
GetBannersForStorefrontAsync()
GetStoriesForStorefrontAsync()
GetThemedPageForStorefrontAsync()

Adapt names to the actual architecture.

The repository should perform the database projection when appropriate.

Prefer:

SQL Server
→ EF projection
→ DTO

rather than:

SQL Server
→ Full EF entity graph
→ Service
→ DTO

--------------------------------------------------
# SERVICE LAYER
--------------------------------------------------

The service should expose use-case-specific storefront reads.

Avoid exposing giant entity graphs to controllers.

Example:

GetCategoryPageAsync()
GetProductDetailAsync()
GetHomePageAsync()
GetSearchResultsAsync()
GetBrandPageAsync()

where these methods return appropriate read models/ViewModels/DTOs according to the existing architecture.

Do not blindly create one service method for every entity.

Design around actual storefront use cases.

--------------------------------------------------
# CONTROLLER LAYER
--------------------------------------------------

Controllers should not:

- manually filter large collections
- manually sort large collections
- manually traverse entity graphs
- trigger lazy loading
- construct large entity graphs
- perform database querying

The controller should orchestrate the request and pass an appropriate ViewModel to the view.

--------------------------------------------------
# ADMIN MUST REMAIN CORRECT
--------------------------------------------------

This optimization primarily targets STOREFRONT reads.

Admin has different requirements.

Admin may legitimately require:

- inactive Products
- inactive Tags
- inactive FileStorage
- full entity graphs
- tracked entities
- edit/update/delete
- SaveChanges()

Do not globally replace admin entity queries with storefront DTOs.

Do not globally change existing repository behavior if it is shared with admin.

If storefront and admin have fundamentally different requirements, create separate methods.

For example:

Storefront:
GetProductForStorefrontAsync()

Admin:
GetProductForAdminEditAsync()

Do not add endless boolean parameters such as:

isActive
includeTags
includeFiles
includeImages
includeBrand
includeCategory
includeSpecifications
includeSomethingElse

to solve fundamentally different use cases.

Prefer explicit use-case methods.

--------------------------------------------------
# READ VS WRITE
--------------------------------------------------

Storefront:

READ
→ AsNoTracking()
→ projection
→ DTO/read model
→ ViewModel

Admin write:

READ tracked entity
→ modify entity
→ SaveChanges()

Do not replace tracked entities with DTOs in workflows that require updates.

--------------------------------------------------
# CACHING AUDIT
--------------------------------------------------

Identify relatively static storefront data:

- Menus
- Categories
- Brands
- Tags
- Filter definitions
- Sorting definitions
- Banners
- Theme configuration
- Static content
- Footer
- site configuration

Determine whether caching already exists.

If caching exists, use the existing mechanism.

If caching does not exist, identify high-value candidates.

Do not introduce caching blindly.

For every caching candidate determine:

Cache key
TTL
Language dependency
Site/tenant dependency
Invalidation strategy

--------------------------------------------------
# SQL SERVER OPTIMIZATION
--------------------------------------------------

After identifying important storefront queries, inspect generated SQL.

Look for:

- SELECT *
- unnecessary joins
- table scans
- clustered index scans
- key lookups
- large sorts
- excessive logical reads
- cartesian multiplication
- duplicate rows
- unnecessary columns
- inefficient predicates

Use SQL Server execution plans where possible.

Potentially important columns may include:

ProductCategoryId
CategoryId
BrandId
TagId
IsActive
Lang
ParentId
Position
Slug
ProductCode
CreatedDate

BUT DO NOT automatically create indexes.

Only recommend/create indexes based on actual query patterns and execution plans.

Document each index:

Index name
Table
Columns
Query benefiting from it
Reason
Potential write/storage cost

--------------------------------------------------
# CARTESIAN EXPLOSION
--------------------------------------------------

Be especially careful when loading multiple collections simultaneously.

For example:

Product
+
ProductFiles
+
ProductTags
+
Gallery
+
Specifications

can create large joined result sets.

Example:

1 product
20 files
10 tags
15 gallery records

can potentially produce a very large multiplication of rows.

Do not assume:

"one SQL query = best performance."

Measure.

A few targeted queries can sometimes be better than one enormous query.

Use projections and appropriate query shapes.

--------------------------------------------------
# ENTITY MATERIALIZATION
--------------------------------------------------

For read-only storefront queries, avoid materializing complete entities when unnecessary.

Bad:

var products = await db.Products
    .Include(...)
    .Include(...)
    .Include(...)
    .ToListAsync();

then map to DTO.

Better:

var products = await db.Products
    .AsNoTracking()
    .Where(...)
    .Select(p => new StorefrontProductDto
    {
        ...
    })
    .ToListAsync();

--------------------------------------------------
# LARGE COLUMNS
--------------------------------------------------

Pay special attention to:

Description
LongDescription
Content
HTML
JSON
Serialized configuration
Email/template content
large metadata

Do not retrieve large fields for listing/card pages unless actually required.

Example:

Product listing:

Id
Name
Price
Thumbnail

Product detail:

Id
Name
Description
Gallery
Specifications
etc.

Blog listing:

Id
Title
Slug
Summary
Thumbnail

Blog detail:

Id
Title
Content
etc.

--------------------------------------------------
# LANGUAGE / LOCALIZATION
--------------------------------------------------

Inspect the existing language implementation.

If data has language fields such as:

Lang
Language
Culture

make sure storefront queries only retrieve the relevant language where the existing business rules require it.

Do not accidentally load all translations.

Do not invent a new localization architecture.

--------------------------------------------------
# IMPORTANT: SEARCH ACTUAL VIEW USAGE
--------------------------------------------------

Do not decide DTO fields based only on entity definitions.

For every important view:

Search exactly which properties are accessed.

For example:

@Model.Product.Name
@Model.Product.Price
@Model.Product.MainImage.Url

Then determine whether:

Product
MainImage

are all that is required.

Also inspect:

- partials
- nested partials
- layout
- helper methods
- AJAX
- JSON serialization

A property used indirectly still counts as required.

--------------------------------------------------
# DO NOT BREAK JSON/AJAX CONTRACTS
--------------------------------------------------

Some storefront endpoints may return JSON consumed by JavaScript.

Before changing a DTO/entity response:

Search JavaScript usage.

Do not remove or rename properties without updating consumers.

Preserve API behavior unless the endpoint is explicitly being redesigned.

--------------------------------------------------
# PERFORMANCE MEASUREMENT
--------------------------------------------------

For important storefront pages measure BEFORE and AFTER.

At minimum inspect:

Homepage
Category page
Product listing
Product detail
Search
Brand page
Tag page
Content page

Measure where practical:

- SQL query count
- SQL execution time
- request duration
- rows returned
- logical reads
- CPU time
- memory/materialization impact

Use:

SET STATISTICS IO ON;
SET STATISTICS TIME ON;

where appropriate.

Inspect actual execution plans for important queries.

Do not claim performance improvements without evidence.

--------------------------------------------------
# IMPLEMENTATION PROCESS
--------------------------------------------------

PHASE 1
========

Audit only.

Do not modify code yet.

Identify:

- storefront services
- storefront repositories
- controllers
- views
- partials
- entities
- Include chains
- large entity graphs
- N+1 queries
- lazy loading
- C# filtering
- C# sorting
- C# pagination
- full entity usage
- existing DTOs
- existing ViewModels
- caching
- shared repository methods
- admin dependencies

Produce a concise audit report.

PHASE 2
========

Design the optimized data flow for each high-value storefront use case.

For example:

Homepage
→ HomePageStorefrontDto/ViewModel

Category
→ CategoryPageStorefrontDto/ViewModel

Product detail
→ ProductDetailStorefrontDto/ViewModel

Search
→ SearchResultsStorefrontDto/ViewModel

etc.

PHASE 3
========

Implement the highest-value optimizations first.

Prioritize:

1. Huge Include graphs
2. Full entities passed to views
3. N+1 queries
4. Large product listings
5. Search queries
6. Category queries
7. Product detail queries
8. Homepage queries
9. Menus
10. Banners/content
11. Other high-traffic storefront operations

PHASE 4
========

Build the entire solution.

Fix compilation issues.

PHASE 5
========

Run existing tests.

Run storefront tests.

Run admin tests.

PHASE 6
========

Measure important queries.

Compare before/after.

PHASE 7
========

Only then consider secondary optimizations such as indexes or caching.

--------------------------------------------------
# EXAMPLE TARGET
--------------------------------------------------

For this existing pattern:

includeProperties.Add(r => r.Products);
includeProperties.Add(r => r.Products.Select(t => t.MainImage));
includeProperties.Add(r => r.Products.Select(t => t.ProductFiles.Select(q => q.FileStorage)));
includeProperties.Add(r => r.Products.Select(t => t.ProductTags.Select(q => q.Tag)));

do NOT simply add more Include() calls.

For storefront:

Category
    ↓
Products WHERE Product.IsActive
    ↓
Project only required Product fields
    ↓
MainImage
    ↓
ProductFiles → FileStorage WHERE FileStorage.IsActive
    ↓
ProductTags → Tag WHERE Tag.IsActive

The relationship entities are not independently filtered by IsActive unless the actual domain logic proves that they should be.

--------------------------------------------------
# QUALITY RULES
--------------------------------------------------

1. Preserve existing business rules.
2. Preserve storefront behavior.
3. Preserve admin behavior.
4. Do not invent activation semantics.
5. Main entity activation is important.
6. Relationship-table activation is NOT automatically important.
7. Do not use filtered Include() in EF6.
8. Use projection for filtered child collections.
9. Use AsNoTracking() for read-only queries.
10. Filter in SQL.
11. Sort in SQL.
12. Paginate in SQL.
13. Avoid N+1.
14. Avoid lazy loading from Razor.
15. Avoid loading entire entities unnecessarily.
16. Avoid giant DTOs.
17. Avoid giant ViewModels.
18. Do not perform unrelated refactoring.
19. Do not change framework versions.
20. Do not introduce async MVC child-action regressions.
21. Do not break JSON/AJAX contracts.
22. Inspect actual callers before changing repository/service contracts.
23. Inspect actual Razor usage before removing properties.
24. Inspect generated SQL.
25. Measure important performance changes.
26. Use indexes only when justified by real queries/execution plans.
27. Keep storefront read models separate from admin write models when appropriate.
28. Prefer explicit use-case-specific read methods over endless boolean Include flags.
29. Do not optimize only ProductCategory; audit the entire storefront.
30. Make incremental, reviewable changes.

--------------------------------------------------
# FINAL REPORT REQUIRED
--------------------------------------------------

When finished, provide:

## 1. Storefront Audit

List all major storefront data flows discovered.

## 2. Problems Found

For each:

- location
- current behavior
- performance problem
- severity
- recommended solution

## 3. DTOs / Read Models

List every DTO/read model added or changed and why.

## 4. ViewModels

List every ViewModel added/changed and which view consumes it.

## 5. Repository Changes

List every repository method added/changed.

## 6. Service Changes

List every service method added/changed.

## 7. Controller Changes

List every controller/action changed.

## 8. Razor Changes

List every affected view/partial/layout.

## 9. Activation Rules

Document which MAIN entities are filtered by activation and explicitly confirm that relationship entities were not incorrectly treated as visibility entities.

Example:

Product.IsActive → filtered
FileStorage.IsActive → filtered
Tag.IsActive → filtered
ProductFile.IsActive → not used
ProductTag.IsActive → not used

Only report rules confirmed from the actual domain model.

## 10. N+1 Problems

List detected and fixed N+1 queries.

## 11. Include Problems

List large/unnecessary Include graphs removed or replaced.

## 12. SQL Improvements

Report important query changes.

## 13. Performance Measurements

Provide BEFORE vs AFTER where measurable:

Query count
SQL time
Request time
Rows
Logical reads
CPU

## 14. Caching Opportunities

List implemented/recommended caching.

## 15. Index Recommendations

Only include evidence-based index recommendations.

## 16. Remaining Work

List high-value optimizations discovered but intentionally not implemented.

--------------------------------------------------
# FINAL ARCHITECTURAL GOAL
--------------------------------------------------

The final storefront architecture should generally look like:

STORE FRONT READ

Controller
    ↓
Storefront Service
    ↓
Purpose-specific Read Repository
    ↓
EF6 AsNoTracking()
    ↓
SQL Projection
    ↓
SQL Server
    ↓
Only required rows/columns
    ↓
DTO / Read Model
    ↓
ViewModel
    ↓
Razor

ADMIN WRITE

Controller
    ↓
Admin Service
    ↓
Repository
    ↓
Tracked EF Entity
    ↓
Modify
    ↓
SaveChanges()

The central rule is:

"A storefront request must retrieve the minimum data required to render that request, while applying the existing business visibility rules in SQL."

Most importantly:

"Filter the activation state of the actual MAIN BUSINESS ENTITY. Do not incorrectly use the activation state of junction/relationship records as the visibility rule."

Do not start implementation until you have inspected the actual codebase and understand the storefront data flows.
