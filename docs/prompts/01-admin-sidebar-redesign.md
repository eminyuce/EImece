# Admin sidebar redesign

- **Captured:** 2026-08-04 12:57:10 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

You are a senior front-end / ASP.NET MVC engineer. Redesign the EImece Admin Area UI so navigation is a fixed left sidebar (not top horizontal menus).

## Project context
- ASP.NET MVC 5, Bootstrap 3, jQuery, Font Awesome 4, Material Icons
- Admin area: Areas/Admin/
- Layout: Areas/Admin/Views/Shared/_Layout.cshtml
- _ViewStart.cshtml already points all admin views to that layout
- Admin CSS via ~/Content/admincss bundle
- Resources: AdminResource.* (Turkish/localized labels)
- Auth helper: UserRoleHelper.IsAdminManagementRoles() for Settings / Users sections
- Controllers (area = "admin"): Dashboard, Customers, Menus, MainPageImages, Products, ProductCategories, Templates, Lists, Brands, Coupons, Orders, Report, ShoppingCarts, Tags, TagCategories, Stories, StoryCategories, Faq, Subscribers, MailTemplates, Settings, AdminSettings, Metrics, Users, Media, FileUpload, Images, ImportData, ProductComments, AppLogs, etc.

## Current problem
_Layout.cshtml uses TWO top navbars (#nav-1 and #nav-2):
- nav-1: company name, user greeting, languages, features, clear cache, home, website, logout
- nav-2: horizontal menu + dropdowns for Products, Tags, Stories, Settings, Users

This wastes vertical space and does not scale. Replace it with a modern left-sidebar admin shell.

## Design goals
1. *Left fixed sidebar* for all primary navigation (collapsible on mobile).
2. *Slim top bar* only for: page context, user menu, language switcher, clear cache, view site, logout — NOT the main nav links.
3. Content area to the right of the sidebar (margin-left / flex layout).
4. Active menu item highlighted based on current controller/action.
5. Grouped menu sections with icons (Font Awesome 4 is already loaded).
6. Keep existing AdminResource strings and role checks.
7. Do not break grids, modals, CKEditor, MVCGrid, or existing partials (_StatusMessage, etc.).
8. Stay on Bootstrap 3 + existing stacks (no React/Vue, no Bootstrap 5 migration unless necessary and isolated).

## Target layout structure

html
<body class="admin-app">
  <aside class="admin-sidebar" id="adminSidebar">
    <!-- brand / logo -->
    <!-- nav groups -->
  </aside>

  <div class="admin-main">
    <header class="admin-topbar">
      <!-- hamburger (mobile), title, languages, user dropdown -->
    </header>
    <main class="admin-content">
      @Html.Partial("_StatusMessage")
      @RenderBody()
    </main>
  </div>
</body>


## Sidebar menu structure (use AdminResource labels where they exist)

*Dashboard*
- Admin home → Dashboard/Index

*Catalog*
- Products → Products/Index
- Product categories → ProductCategories/Index
- Brands → Brands/Index
- Templates → Templates/Index
- Spec lists → Lists/Index
- Coupons → Coupons/Index

*Sales*
- Orders → Orders/Index
- Customers → Customers/Index
- Shopping carts → ShoppingCarts/Index
- Reports → Report/Index

*Content*
- Menus → Menus/Index
- Main page images → MainPageImages/Index
- Stories → Stories/Index
- Story categories → StoryCategories/Index
- Tags → Tags/Index
- Tag categories → TagCategories/Index
- FAQ → Faq/Index
- Subscribers → Subscribers/Index

*Media* (if routes exist)
- Media / File upload / Images as applicable

*System* (only if UserRoleHelper.IsAdminManagementRoles())
- Mail templates → MailTemplates/Index
- Website logo → Settings/AddWebSiteLogo
- Admin settings → AdminSettings/Index
- System settings → AdminSettings/SystemSettings
- Metrics → Metrics/Index
- Health → /Health (external/blank target as today)
- Users → Users/Index
- Change password → Users/changepassword

*Top bar actions* (not in sidebar)
- Languages (Html.Action("Languages", "Dashboard") or equivalent)
- Our site features
- Clear cache
- View website (/ target blank)
- Logout (existing anti-forgery LogOff form)

## Implementation requirements

### 1. Rewrite Areas/Admin/Views/Shared/_Layout.cshtml
- Remove both top navbars as the primary navigation.
- Implement sidebar + topbar + content shell as above.
- Preserve scripts/styles currently in <head> (jquery, bootstrap, adminScripts, admincss, CKEditor, MVCGrid, Font Awesome, Material Icons).
- Keep logout form with @Html.AntiForgeryToken().
- Keep setlanguage JS if still needed.

### 2. Add CSS (prefer admin-specific file in Content / admincss bundle)
- Fixed sidebar width ~240–260px (collapsed ~64px optional).
- admin-main offset for sidebar.
- Sticky topbar.
- Scrollable sidebar if menu is long.
- Active link style (compare current controller name).
- Hover states, section headers, subtle dividers.
- Mobile: sidebar off-canvas; hamburger toggles .sidebar-open on body or sidebar; overlay optional.
- Do not break existing grid/table layouts inside content.

### 3. Active state helper
- In layout Razor, detect current controller (and optionally action) via ViewContext.RouteData.Values["controller"].
- Add CSS class active to the matching sidebar link.
- Optional: expand parent group when a child is active.

### 4. Partial for menu (recommended)
- Extract sidebar to Areas/Admin/Views/Shared/_AdminSidebar.cshtml
- Extract topbar to _AdminTopbar.cshtml if it keeps layout clean
- Layout only composes the shell

### 5. Responsive behavior
- ≥992px: sidebar always visible
- <992px: sidebar hidden by default; toggle button in topbar
- Ensure tables/forms still usable on small screens

### 6. Visual quality bar
- Clean, modern admin look (neutral sidebar, clear hierarchy)
- Consistent spacing; icons left of labels
- Company name from existing SettingService / Constants.CompanyName logic in sidebar brand
- Show logged-in user name in topbar (link to change password as today)

### 7. Constraints
- Do not change controller actions or routes.
- Do not remove role checks around Settings/Users.
- Do not migrate to Bootstrap 5 unless you isolate styles carefully and document it.
- Prefer minimal JS: sidebar toggle only (vanilla or existing jQuery).
- Keep Turkish/resource-driven labels; no hard-coded English where AdminResource exists.
- Preserve @RenderSection("scripts") and @RenderSection("Styles").

## Deliverables
1. Updated _Layout.cshtml
2. New partial(s): _AdminSidebar.cshtml (± _AdminTopbar.cshtml)
3. New/updated admin CSS for the shell + responsive sidebar
4. Small JS for mobile toggle if needed
5. Short note on how active menu detection works and any bundle updates

## Acceptance criteria
- No primary admin links remain in a top horizontal menu bar.
- All previous nav destinations remain reachable from the left sidebar.
- Settings/Users groups still gated by UserRoleHelper.IsAdminManagementRoles().
- Active page is visually indicated in the sidebar.
- Mobile usable via collapsible sidebar.
- Existing admin pages (products grid, orders, dashboard, etc.) render correctly in the content area.

Start by reading the current _Layout.cshtml, then implement the shell and move every existing menu link into the grouped sidebar.
```

---

*How to use it*
1. In Cursor/Copilot, open the Admin area and @ reference:
   - Areas/Admin/Views/Shared/_Layout.cshtml
   - Areas/Admin/Views/_ViewStart.cshtml
   - admin CSS/bundle files if you know the path
2. Paste the prompt and ask for the layout rewrite first, then CSS polish.
3. If you want a denser look, add: “Collapsed icon-only sidebar on desktop with expand on hover.”
4. If you prefer a specific style, add: “Visual style similar to AdminLTE 2 / CoreUI (Bootstrap 3 era).”
