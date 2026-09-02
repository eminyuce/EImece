# Align admin list pages with Products UI

- **Captured:** 2026-08-10 11:05:41 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

You are modernizing EImece admin list pages to match the Products admin UI as the single visual/behavioral reference.
## Reference (source of truth)
Treat these as the canonical pattern. Do not invent a new design system.
- Page: /admin/products/ → Areas/Admin/Views/Products/Index.cshtml
- Shared grid shell: Views/Shared/_Grid.cshtml, Views/Shared/_GridPager.cshtml
- Shared ops/search: Areas/Admin/Views/Shared/pGridOperations.cshtml, pAdminSearchForm.cshtml
- Shared grid cells/actions: Areas/Admin/Views/Shared/Grid/*
- Styles: Content/adminGridModern.css
- Behavior: Scripts/adminGridModern.js
- Category tree (when applicable): _ProductCategoryTree.cshtml, _ProductCategoryTreeChildren.cshtml
## Goal
Bring other admin Index/list pages to the same UI quality as Products:
- same visual language (spacing, borders, radii, colors, typography)
- same toolbar/search/bulk-action patterns
- same Grid.Mvc table chrome, sticky/action patterns where relevant
- same pager footer (summary + controls) outside any inner scroll
- page-level scrolling only (no inner vertical grid scroll)
- reuse shared partials/CSS/JS; avoid page-specific one-off styling unless required by domain
## Hard constraints
1. Keep existing controller action names, route params, element IDs used by JS (SelectAll, DeleteAll, SetStateOnAll, SetStateOffAll, OrderingAll, searchTxtInput, SearchButton, grid checkbox names, Ajax endpoints, etc.).
2. Prefer extending shared partials (pGridOperations, Grid/*, _Grid, _GridPager) over copying markup into each page.
3. Do not break non-list admin pages (Dashboard, Settings, edit forms) unless they share the same components.
4. Preserve Turkish resource strings / AdminResource usage.
5. Include new views/assets in EImece.csproj if needed so IIS publish includes them.
6. After changes: deploy/copy to local IIS (C:\inetpub\wwwroot\Eimece) when working against that environment, hard-refresh, and verify the page loads without exceptions.
7. Match Products denseness: primary row larger; second-line bulk toolbar slightly smaller.
## Implementation checklist per admin list page
For each target Index page (Brands, Stories, Menus, Tags, Coupons, Orders, Customers, Media, etc.):
1. Compare with Products Index structure:
   - primary ops row (new + search [+ export if present])
   - second-line bulk toolbar via pGridOperations
   - Grid/_GridChrome density/selected count when using modern grid
   - @Html.Grid(...).Columns(...) using shared Grid/* partials for image/name/status/actions when entity allows
   - pager via shared _Grid/_GridPager
2. Replace legacy inline action button columns with Grid/_GridActionsMenu pattern where delete/edit/media actions exist.
3. Ensure adminGridModern.css / adminGridModern.js cover the page (already bundled); add page-specific CSS only under existing eg-/admin-grid-* conventions.
4. Remove/avoid max-height inner scroll on grids; page should scroll as a whole.
5. If the page has a left tree/nav, follow the Products category-tree UX (eg-category-tree) only when it fits; otherwise leave as-is.
6. Visual QA with screenshots for toolbar, grid header/rows, actions menu, pager.
7. Fix defects, redeploy, retest until polished.
## Suggested rollout order
1. Brands, Stories, Menus, Tags, TagCategories, StoryCategories
2. Coupons, Faq, MailTemplates, Lists, Templates, MainPageImages
3. ProductCategories, ProductComments, Subscribers
4. Customers, Orders, ShoppingCarts, Media, Users, AppLogs
## Definition of done
- Target page visually consistent with Products admin list
- No missing partial/view publish issues
- Existing bulk/search/delete/state JS still works
- Pager shows range/total and works
- No inner vertical grid scrollbar; page scrolls normally
