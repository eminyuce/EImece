# Audit all DTOs used by Razor views

- **Captured:** 2026-08-22 7:39:04 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

Audit and refactor *ALL DTOs used by Razor Views across the entire EImece application*.

Do *not* limit the audit to EImece.Domain.Models.DTOs.Storefront or any single DTO package/namespace.

For *every Razor View* under:

text
EImece\Views
EImece\Views\Designs\Modern
EImece\Views\Designs\Crizal
EImece\Areas\Customers


trace its complete data flow:

text
Razor View
→ ViewModel
→ DTO
→ Service
→ Repository
→ EF6 Query
→ Database


For every DTO used by a Razor View:

1. Determine the *exact properties actually required* by the View and its rendering/operation.
2. Remove every DTO property that is not required.
3. If a DTO is shared by different Views and contains fields required by only some consumers, create purpose-specific DTOs where appropriate.
4. Update Services, Repositories, AutoMapper mappings, and EF6 queries accordingly.
5. Ensure EF6 queries use direct .Select() projections and retrieve *only the required database columns*.
6. Do not load a full Entity and then map it to a smaller DTO.
7. Remove unnecessary .Include() calls.
8. ViewModels must never contain Entity classes.
9. Do not assume a DTO is optimized simply because it is a DTO. A DTO with unnecessary fields must also be refactored.

Example:

If a Razor View only uses:

text
Product.Id
Product.Name
Product.Code


the DTO and query should contain only those fields:

csharp
.Select(x => new ProductDto
{
    Id = x.Id,
    Name = x.Name,
    Code = x.Code
})


Do *not* retrieve the complete Product Entity and map it afterward.

Also distinguish between fields needed to *find/filter* a record and fields required in the DTO:

csharp
.Where(x => x.SettingKey == key)
.Select(x => new SettingValueDto
{
    SettingValue = x.SettingValue
})


SettingKey is required for the query but is not required in the DTO unless the consumer actually uses it.

### Critical Requirement

This is an *exhaustive audit*, not a sample review.

Do not fix only the first few DTOs you encounter.

Start by discovering *all Razor Views and all DTOs used by those Views, then process them systematically until **100% of the applicable DTOs have been reviewed*.

The goal is:

text
Razor View
    ↓
Minimal ViewModel
    ↓
Minimal DTO
    ↓
Minimal EF6 Projection
    ↓
Only required database columns


Minimize:

* Database columns retrieved
* Database I/O
* SQL result size
* Network traffic
* EF Entity materialization
* Memory usage
* Unnecessary joins
* Unnecessary .Include()
* Unnecessary object mapping

Preserve existing functionality and UI behavior. Do not perform unrelated refactoring.

### Completion Requirement

Before finishing, verify that:

* Every Razor View in scope has been analyzed.
* Every DTO used by those Views has been analyzed.
* Every DTO property has been checked for actual usage.
* Unnecessary DTO properties have been removed.
* Full Entities are not being loaded unnecessarily.
* EF6 queries retrieve only required columns.
* Unnecessary .Include() calls are removed.
* ViewModels do not contain Entity classes.
* All affected Services, Repositories, and mappings have been updated.
* The audit is complete across the *entire application*, not just one DTO namespace or package.

Do not stop after finding a few examples. *Continue until the complete Razor View → DTO → database data flow has been audited and optimized.*
