# Caching: Storefront vs Admin

- **Storefront**: may use cache-based service methods for performance and fewer DB round-trips.
- **Admin panel**: must always fetch the latest data from the database. Do not use cached service entry points for Admin screens or Admin writes.

When adding or refactoring services, keep this split explicit (method naming / comments / separate APIs). Caching added for storefront must not silently affect Admin.

Example: `TemplateService.GetTemplate` / `GetTemplateAsync` always hit the database (used by Admin product specs). `GetAllActiveTemplates` may cache for storefront-style bulk reads and must not be used as the Admin source of truth.

## Admin cache fixes (`fix/product-specs-template-null`)

### Menu / ProductCategory trees
- `BuildTree` / `BuildTreeAsync`: when `isActive` is `null` (Admin), always hit the DB.
- Storefront/warmup pass `true`/`false` and may still cache under `menu:tree:…` / `category:tree:…`.

### Settings
- Admin settings pages (`GetSettingModel*`, `GetSystemSettingModel*`, and their saves) always use `GetAllSettingsNoCache*`.
- Admin controllers/views use `GetSetting*FromDb*` so individual key reads never use the storefront settings cache.
- Storefront continues to use cached `GetSettingByKey*` / `GetAllSettings*`.

### Templates
- `GetTemplate` / `GetTemplateAsync` always DB (Admin-safe).
- `GetAllActiveTemplates` may still cache for storefront.
