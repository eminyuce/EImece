# Implement target architecture safely

- **Captured:** 2026-08-20 11:52:52 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

Here is a complete, copy-pasteable *AI Coding Prompt* designed to implement this architecture cleanly and safely.

---

markdown
### Task: Migrate High-Value Web.config Settings to Admin Panel (Settings Table) with Fallback

#### Objective
Implement dynamic runtime system configuration by migrating high-value business, UI, and content settings from `Web.config` into the database (`Settings` table) while preserving complete backward compatibility using the **Database-First with Web.config Fallback** pattern.

---

### Architectural Requirements

1. **Resolution Pattern (Database-First, Config-Fallback)**:
   - When reading a setting, first query `SettingService.GetSettingByKeyAsync(...)` (or its cached helper).
   - If the database value is null, empty, or whitespace, fall back to the existing value in `AppConfig` / `Web.config`.
   - Never break existing deployments if the database rows do not exist yet.

2. **Caching & Invalidation**:
   - Utilize existing `SettingService` memory caching for fast reads without hitting SQL on every request.
   - Automatically invalidate or update cache when settings are saved in the Admin panel.

3. **Admin UI Management**:
   - Provide a clean, categorized, responsive view in `Areas/Admin` (e.g., `Areas/Admin/Views/Settings/SystemSettings.cshtml`) with logical tabs/groups.
   - Use strongly typed ViewModels with proper validation attributes (ranges, required fields, regex, etc.).

4. **Security Bounds**:
   - Never store unencrypted secrets in the settings table.
   - Keep `SiteStatus` (dev/staging/live), connection strings, and crypto keys strictly in `Web.config` / environment variables.

---

### List of Settings to Migrate

#### 1. Site Maintenance & SEO
* `IsSiteUnderConstruction` (Boolean toggle: true = maintenance page & noindex)
* `AllowSearchEngineIndexing` (Boolean toggle: true = allow search engine crawling)
* `ActiveDesign` (String dropdown: `Crizal`, `Modern`, or other registered skins)

#### 2. PWA & Web App Manifest Branding
* `ThemeColor` (Hex color string, e.g., `#1789F9`)
* `ManifestBackgroundColor` (Hex color string, e.g., `#ffffff`)
* `ManifestDisplay` (Dropdown: `standalone`, `fullscreen`, `minimal-ui`, `browser`)
* `ManifestOrientation` (Dropdown: `portrait`, `landscape`, `any`)
* `ManifestStartUrl` (String, default `/`)
* `ManifestFallbackName` (String, e.g., `Web App`)
* `ManifestShortNameMaxLength` (Integer, default `12`)

#### 3. Admin & Content UI Preferences
* `GridPageSizeNumber` (Integer: items per page in admin tables, e.g., 20, 50, 100)
* `ProductShortDescriptionPreviewLength` (Integer: character preview length before "Continue...")
* `IsEditLinkEnable` (Boolean toggle: show front-end edit shortcuts for logged-in editors)
* `AdminImageHeightPercantage` (Integer: admin media thumbnail height percentage)
* `AdminImageWidthPercantage` (Integer: admin media thumbnail width percentage)

#### 4. Media & Image Upload Policies
* `ImageUploadMaxWidth` & `ImageUploadMaxHeight` (Integer px: max boundary box, e.g., 1920)
* `ImageUploadJpegQuality` (Integer 40–95: JPEG compression quality, default 82)
* `ImageUploadPreferWebP` (Boolean toggle: convert uploaded images to WebP)
* `ImageUploadWebPQuality` (Integer 40–100: WebP quality)
* `ImageUploadSaveWebPSidecar` (Boolean toggle: write .webp sidecar next to original)
* `ImageUploadThumbMaxWidth` & `ImageUploadThumbMaxHeight` (Integer px: thumbnail cap)
* `ImageUploadThumbJpegQuality` (Integer 40–95: thumbnail JPEG quality)
* `ImageUploadKeepOriginalIfSmaller` (Boolean toggle: retain original if re-encoding doesn't reduce size)

#### 5. Payments & E-Commerce Options
* `PaymentProvider` (String / Dropdown: e.g., `Iyzico`)
* `IyzicoEnabledInstallments` (Comma-separated integers: e.g., `1,2,4,6,9`)
* `BuyerIdentityNumber` (String: fallback sandbox TCKN/tax ID)

#### 6. Captcha & Anti-Spam
* `CaptchaProvider` (Dropdown: `Legacy`, `Recaptcha`, `None`)
* `RecaptchaSiteKey` (String: public Google reCAPTCHA site key)
* `RateLimit:Enabled` (Boolean toggle: enable/disable rate limiting middleware)
* `RateLimit:Login:Limit` & `RateLimit:Login:WindowMinutes` (Integer limit & window)
* `RateLimit:Contact:Limit` & `RateLimit:Contact:WindowMinutes` (Integer limit & window)
* `RateLimit:Checkout:Limit` & `RateLimit:Checkout:WindowMinutes` (Integer limit & window)
* `RateLimit:Search:Limit` & `RateLimit:Search:WindowMinutes` (Integer limit & window)

---

### Step-by-Step Implementation Plan

1. **Constants**:
   - Register all keys in `EImece.Domain/Constants.cs` under the setting keys section.

2. **Domain Service / Helper Layer**:
   - Update `AppConfig.cs` and `SettingService.cs` helper methods to read DB values first via `SettingService` before falling back to `ConfigurationManager.AppSettings`.

3. **Admin Controller & ViewModel**:
   - Create/update `Areas/Admin/Controllers/SettingsController.cs` with `GET` and `POST` actions for `SystemSettings`.
   - Implement `SystemSettingsViewModel` grouping the settings into tabs:
     - *General & SEO*
     - *Design & PWA*
     - *Media & Uploads*
     - *Payments & Captcha*
     - *Rate Limiting*

4. **Admin Razor View**:
   - Create `Areas/Admin/Views/Settings/SystemSettings.cshtml` matching the existing Bootstrap 5 admin theme, with toggle switches, input validation, help tooltips, and success toast notifications.

5. **Unit Tests**:
   - Add unit tests in `EImece.Tests` validating that:
     1. DB values override `Web.config` when present.
     2. `Web.config` values are returned when the DB setting is absent.
     3. Cache invalidation functions properly upon saving.
