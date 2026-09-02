# Remove entity classes from ViewModels

- **Captured:** 2026-08-21 7:08:47 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

In our ASP.NET MVC application, several ViewModels (especially in the Store Front and Customer pages) currently contain full Entity classes. This is incorrect and must be fixed.

*Problem*
- ViewModels must never contain or depend on Entity classes.
- Loading entire entities pulls unnecessary columns from the database, increasing network traffic, memory usage, and query cost.
- Example: If a view only needs Product.Id and Product.Name, we must not load the full Product entity.

*Goal*
Audit and fix *all* ViewModels and their corresponding views in EImece\Views (and Views\Designs\Modern and Views\Designs\Crizal  ) and EImece\Areas\Customers:
- Store Front pages
- Customer pages

Replace every Entity class with a dedicated DTO that contains *only the fields required by the view*.

*Rules*
1. ViewModels may only contain DTOs or simple/primitive types — never Entity classes.
2. Create new DTOs or reuse existing ones that project only the required fields.
3. Update the related queries, repository methods, and mapping logic (AutoMapper, Select projections, etc.) so only those fields are fetched.
4. Completely remove all Entity class references from ViewModels.

*Desired structure*
ViewModel
  └── DTO1 (only required fields)
  └── DTO2 (only required fields)
  └── DTO3 (only required fields)

Start by listing every ViewModel in the Store Front and Customer areas that currently contains Entity classes, then propose the DTO changes and the necessary code updates.
