# Caching: Storefront vs Admin

- **Storefront**: may use cache-based service methods for performance and fewer DB round-trips.
- **Admin panel**: must always fetch the latest data from the database. Do not use cached service entry points for Admin screens or Admin writes.

When adding or refactoring services, keep this split explicit (method naming / comments / separate APIs). Caching added for storefront must not silently affect Admin.

Example: `TemplateService.GetTemplate` / `GetTemplateAsync` always hit the database (used by Admin product specs). `GetAllActiveTemplates` may cache for storefront-style bulk reads and must not be used as the Admin source of truth.