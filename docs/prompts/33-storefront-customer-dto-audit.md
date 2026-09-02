# Audit Store Front and Customer areas onto DTOs

- **Captured:** 2026-08-22 6:45:23 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

# Role

You are a *Senior ASP.NET MVC 5 / .NET Framework / Entity Framework 6 performance engineer* performing a comprehensive architecture and performance refactoring of the EImece application.

# Objective

Audit and refactor the *Store Front* and *Customer* areas of the EImece ASP.NET MVC application so that:

1. ViewModels *never contain Entity classes*.
2. ViewModels contain only *DTOs and simple/primitive types*.
3. Every DTO contains *only the fields actually required* by its corresponding Razor View or operation.
4. Database queries retrieve *only the required columns*.
5. Full Entity objects are not loaded when only a subset of fields is required.
6. Existing application behavior and UI functionality must remain unchanged.

The goal is *not merely to replace Entity classes with DTO classes*. The DTOs themselves must be minimal and purpose-specific.

---

# Important Principle

A DTO containing unnecessary properties is *still an architectural and performance problem*, even though it is technically a DTO.

For every DTO, determine:

> *Which exact properties does the View or operation actually consume?*

If a property is not required, it must not be included in the DTO and must not be fetched from the database.

For example:

If a Razor View only requires:

text
Product.Id
Product.Name


then the application must not retrieve:

text
Product.Description
Product.CreatedDate
Product.UpdatedDate
Product.Price
Product.Stock
Product.Category
...


just because those properties exist on the Entity.

The database query should retrieve only Id and Name.

---

# Concrete Example

I inspected:

text
C:\Users\eminy\source\repos\EImece\EImece\EImece\Views\ProductCategories\Category.cshtml


Suppose the View uses a setting like:

csharp
IsProductReviewEnable.SettingValue.ToBool(true)


In this scenario, the View/operation only requires:

text
SettingValue


However, the current SettingDto contains many unnecessary properties:

csharp
public class SettingDto
{
    // BaseEntity
    public int Id { get; set; }                 // NOT REQUIRED
    public string Name { get; set; }            // NOT REQUIRED
    public DateTime CreatedDate { get; set; }   // NOT REQUIRED
    public DateTime UpdatedDate { get; set; }   // NOT REQUIRED
    public bool IsActive { get; set; }          // NOT REQUIRED
    public int Position { get; set; }           // NOT REQUIRED
    public int Lang { get; set; }               // NOT REQUIRED

    // Setting
    public string Description { get; set; }     // NOT REQUIRED
    public string SettingKey { get; set; }      // NOT REQUIRED — already queried by key
    public string SettingValue { get; set; }    // REQUIRED
}


This should *not* be considered an optimized DTO simply because it is named SettingDto.

If the operation only needs SettingValue, the data-access layer should retrieve only SettingValue.

For example:

csharp
var settingValue = await repository
    .Where(x => x.SettingKey == key)
    .Select(x => x.SettingValue)
    .FirstOrDefaultAsync();


If a DTO is actually required at that boundary, it should contain only:

csharp
public class SettingValueDto
{
    public string SettingValue { get; set; }
}


Do not fetch the entire Setting entity and then map it to SettingValueDto.

---

# Scope

Perform a *complete audit* of:

text
EImece\Views
EImece\Views\Designs\Modern
EImece\Views\Designs\Crizal
EImece\Areas\Customers


Focus especially on:

* Store Front pages
* Product pages
* Product Category pages
* Customer pages
* Customer account pages
* Customer-related partial views
* Shared/partial views used by Store Front or Customer pages
* ViewModels used by those views
* Nested DTOs/entities inside those ViewModels
* Repository methods supplying their data
* Service methods assembling the ViewModels
* AutoMapper profiles/configuration
* EF6 queries and projections

Do not limit the investigation to files whose names contain ViewModel.

Trace the *complete data flow*:

text
Database
   ↓
Repository
   ↓
Service
   ↓
Mapping / Projection
   ↓
ViewModel
   ↓
Razor View


---

# Primary Rule: ViewModels Must Not Contain Entities

Find every ViewModel that directly or indirectly contains an Entity class.

Examples of violations:

csharp
public Product Product { get; set; }
public Category Category { get; set; }
public Setting Setting { get; set; }
public List<Product> Products { get; set; }
public IEnumerable<Category> Categories { get; set; }


These must be replaced with DTOs.

Desired structure:

text
ViewModel
 ├── ProductDto
 ├── CategoryDto
 ├── SettingDto
 └── primitive/simple values


However, *do not blindly reuse an existing large DTO*.

If the existing DTO contains unnecessary properties, create a more specific DTO or projection.

---

# DTO Minimality Rule

For every DTO, perform a property-level usage analysis.

For each property determine:

text
REQUIRED
NOT REQUIRED
REQUIRED ONLY FOR ANOTHER OPERATION
REQUIRED ONLY FOR DATABASE FILTERING
REQUIRED ONLY FOR SORTING
REQUIRED ONLY FOR AUTHORIZATION


Important distinction:

A property may be required for the *database query/filter*, but that does not necessarily mean it should be included in the returned DTO.

For example:

csharp
.Where(x => x.SettingKey == key)
.Select(x => new SettingValueDto
{
    SettingValue = x.SettingValue
})


SettingKey is required to locate the record, but it is *not required in the DTO*.

Do not confuse:

> "required to query the record"

with:

> "required in the returned DTO."

---

# Database Projection Rule

Whenever possible, project directly from EF6 to the required DTO/data shape.

Preferred:

csharp
var products = await db.Products
    .AsNoTracking()
    .Where(x => x.IsActive)
    .Select(x => new ProductListDto
    {
        ProductId = x.Id,
        ProductName = x.Name,
        ProductCode = x.Code
    })
    .ToListAsync();


Avoid:

csharp
var products = await db.Products
    .AsNoTracking()
    .Where(x => x.IsActive)
    .ToListAsync();

var result = products.Select(x => new ProductListDto
{
    ProductId = x.Id,
    ProductName = x.Name,
    ProductCode = x.Code
});


The second approach loads the entire Entity before creating the DTO.

---

# Avoid Unnecessary Includes

Audit all .Include() calls.

If a related Entity is not required by the View/DTO, remove the .Include().

Do not use:

csharp
.Include(x => x.Category)
.Include(x => x.Brand)
.Include(x => x.Supplier)


unless the required View/operation actually consumes data from those relationships.

Prefer explicit projections:

csharp
.Select(x => new ProductDto
{
    Id = x.Id,
    Name = x.Name,
    CategoryName = x.Category.Name
})


This allows EF6 to generate a query that retrieves only the required columns.

---

# AutoMapper Rule

Audit AutoMapper mappings.

Do not assume that using AutoMapper automatically makes the implementation efficient.

For example, this is potentially inefficient:

csharp
var entity = repository.GetProduct(id);
var dto = mapper.Map<ProductDto>(entity);


If the Entity contains 30 columns and the DTO requires only 5, the database may still retrieve all 30 columns.

Prefer query-level projection where practical:

csharp
var dto = query
    .Where(x => x.Id == id)
    .ProjectTo<ProductDto>(configuration)
    .FirstOrDefaultAsync();


or an explicit EF projection:

csharp
.Select(x => new ProductDto
{
    Id = x.Id,
    Name = x.Name,
    Code = x.Code
})


The important requirement is:

> *The optimization must happen at the database query level, not only after Entity materialization.*

---

# Do Not Over-Optimize Incorrectly

Do not remove fields that are genuinely required.

Before removing a property, inspect:

* Razor expressions
* if conditions
* foreach
* partial views
* editor/display templates
* HTML attributes
* JavaScript data attributes
* AJAX-related data
* model binding
* form POST requirements
* validation
* sorting
* filtering
* authorization logic
* localization
* pagination
* child/partial ViewModels
* helper methods
* extension methods
* computed properties
* service-layer operations

A property is considered unnecessary only after verifying that removing it does not change required behavior.

---

# Razor View Analysis

For every View, inspect exactly how the Model is consumed.

For example:

csharp
@Model.Product.Name
@Model.Product.Code
@Model.Product.Price


means those properties are required.

But if:

csharp
Product.Description
Product.CreatedDate
Product.UpdatedDate


are never used anywhere in the View or its required rendering path, they should not be part of the DTO.

Also inspect:

csharp
@Html.DisplayFor(...)
@Html.EditorFor(...)
@Html.Partial(...)
@Html.Action(...)


because required fields may be consumed indirectly by another View or template.

Trace those dependencies before removing properties.

---

# Shared DTO Rule

Do not create a giant "universal" DTO merely to avoid creating multiple DTOs.

For example, avoid:

csharp
ProductDto
{
    Id,
    Name,
    Code,
    Description,
    Price,
    Stock,
    CreatedDate,
    UpdatedDate,
    Category,
    Brand,
    Supplier,
    Images,
    Reviews,
    ...
}


when different pages require different subsets.

Prefer purpose-specific DTOs such as:

text
ProductListItemDto
ProductDetailsDto
ProductSearchResultDto
ProductCategoryItemDto
ProductPriceDto
ProductImageDto


Each DTO should represent the *minimum data contract required by its consumer*.

---

# Performance Requirements

The refactoring must reduce unnecessary:

* Database I/O
* Selected database columns
* SQL result-set size
* Network traffic between application and database
* EF entity materialization
* Application memory usage
* Object allocations
* Serialization/deserialization
* Query execution cost
* .Include() overhead

Do not claim performance improvements merely because an Entity was renamed to a DTO.

The actual database query must also be optimized.

---

# Audit Process

Follow this process systematically.

## Phase 1 — Inventory

First identify *every Store Front and Customer ViewModel* that:

* Contains an Entity directly
* Contains a collection of Entities
* Contains an Entity through another nested object
* Uses an oversized DTO
* Loads more fields than the View requires

Create an inventory before making changes.

## Phase 2 — View Usage Analysis

For each ViewModel:

1. Find the Razor View(s) that consume it.
2. Inspect every property used by the View.
3. Follow partial Views and templates.
4. Identify the exact required data fields.
5. Identify fields that are never consumed.

## Phase 3 — Data Access Analysis

Trace where each field originates.

For every ViewModel determine:

text
View
  ↓
ViewModel
  ↓
Service
  ↓
Repository
  ↓
EF Query
  ↓
Entity / Database


Identify where unnecessary columns are being loaded.

## Phase 4 — DTO Design

Create or modify DTOs so they contain only required fields.

## Phase 5 — Query Optimization

Update:

* Repository methods
* Service methods
* EF6 queries
* LINQ projections
* AutoMapper projections
* .Include() usage
* Mapping logic

so the database retrieves only the required fields.

## Phase 6 — Validation

After refactoring, verify:

* The View renders correctly.
* POST operations still work.
* Model binding still works.
* Partial Views still work.
* AJAX functionality still works.
* Filtering/sorting still works.
* Localization still works.
* No Entity classes remain inside the targeted ViewModels.
* No required fields were accidentally removed.
* Queries still execute correctly.

---

# Important: Do Not Stop at the First Level

If you find:

csharp
public ProductDto Product { get; set; }


do not automatically consider the problem solved.

Inspect ProductDto.

If it contains:

csharp
public int Id { get; set; }
public string Name { get; set; }
public string Code { get; set; }
public string Description { get; set; }
public decimal Price { get; set; }
public DateTime CreatedDate { get; set; }
public DateTime UpdatedDate { get; set; }
public bool IsActive { get; set; }
...


but the View only requires:

text
Id
Name
Code


then ProductDto is still oversized.

The audit must therefore be *recursive and property-level*.

---

# Scope Boundary

Prioritize:

text
EImece\Views
EImece\Views\Designs\Modern
EImece\Views\Designs\Crizal
EImece\Areas\Customers


Focus on Store Front and Customer functionality.

Do not perform unrelated architectural refactoring.

Do not change:

* Business rules
* UI behavior
* URLs/routes
* Authentication behavior
* Authorization behavior
* Database schema

unless absolutely necessary to complete this DTO/projection refactoring.

---

# Required Initial Report

Before modifying code, produce an inventory containing:

| View / ViewModel | Entity/DTO | Current Fields | Required Fields | Unnecessary Fields | Query/Repository | Recommended Change |
| ---------------- | ---------- | -------------- | --------------- | ------------------ | ---------------- | ------------------ |

For every identified problem, show:

1. View path
2. ViewModel class
3. Entity currently being used
4. DTO currently being used, if any
5. Fields actually consumed by the View
6. Unnecessary fields
7. Repository method
8. Service method
9. Current EF query
10. Recommended DTO
11. Recommended projection
12. Expected database columns after refactoring

---

# Example Expected Refactoring

### Before

text
Razor View
    ↓
ViewModel
    ↓
SettingDto
    ├── Id
    ├── Name
    ├── CreatedDate
    ├── UpdatedDate
    ├── IsActive
    ├── Position
    ├── Lang
    ├── Description
    ├── SettingKey
    └── SettingValue


Database query:

text
SELECT *
FROM Settings
WHERE SettingKey = @key


### After

text
Razor View
    ↓
ViewModel
    ↓
SettingValueDto
    └── SettingValue


Database query should effectively retrieve only:

text
SELECT SettingValue
FROM Settings
WHERE SettingKey = @key


The fact that SettingKey is used in the WHERE clause does *not* mean it needs to be returned in the DTO.

---

# Final Acceptance Criteria

The implementation is complete only when:

* [ ] All targeted Store Front ViewModels have been audited.
* [ ] All targeted Customer ViewModels have been audited.
* [ ] No targeted ViewModel directly contains an Entity class.
* [ ] No targeted ViewModel indirectly exposes unnecessary Entity data.
* [ ] Every DTO contains only fields required by its consumer.
* [ ] Oversized DTOs have been split or reduced where necessary.
* [ ] EF6 queries project only required columns.
* [ ] Unnecessary .Include() calls are removed.
* [ ] Repository methods no longer load complete Entities when only a projection is required.
* [ ] AutoMapper does not cause unnecessary Entity materialization where query projection is possible.
* [ ] Existing Razor Views continue to render correctly.
* [ ] Existing functionality remains unchanged.
* [ ] No unrelated refactoring has been introduced.

# Critical Instruction

*Do not treat "Entity → DTO" conversion alone as sufficient.*

The real objective is:

text
View requirements
      ↓
Minimal DTO
      ↓
Minimal LINQ projection
      ↓
Minimal SQL SELECT
      ↓
Minimal database/network/memory cost


For every refactored ViewModel, follow the data all the way from the Razor View back to the database and ensure that *only the data actually required by that View or operation is retrieved*.
