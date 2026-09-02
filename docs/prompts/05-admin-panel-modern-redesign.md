# Modern e-commerce admin panel redesign

- **Captured:** 2026-08-06 4:42:38 PM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

Design a complete modern redesign of an e-commerce admin panel (ASP.NET MVC 5 + Bootstrap 3 + GridMvc).

CURRENT SYSTEM (keep 100% of the functionality and Turkish labels):

Shell:
- Fixed dark left sidebar (250px, collapses to 68px icon-only on desktop)
- Sticky white top bar (42px height)
- Content area on light gray background (#f5f7fa)
- Collapsible nav groups: Katalog, Satışlar, İçerik, Sistem
- Sidebar brand + close button on mobile
- Top bar: panel title, page title, language selector, features, refresh, website link, user greeting, logout

Typical Index / Listing page pattern (used by almost every module):
1. Page title (h2)
2. Optional left category tree (Products, ProductCategories, Stories, etc.)
3. Bulk operations toolbar (panel-info style):
   - Primary “Yeni Giriş / Yeni Kayıt” button
   - Search form
   - Select All / Deselect All
   - Delete Selected (danger)
   - State dropdown + Activate / Deactivate buttons
   - Ordering update button
   - Product State change (products only)
   - Excel + CSV export
4. GridMvc data table with:
   - Checkbox column
   - Row index badge
   - Name column (link + long name + action icons: camera, edit, specs, delete, comments)
   - Category / Brand columns
   - Price column with discount badge + strikethrough
   - Product Code
   - Product State text
   - Position (editable small input)
   - Status icons (Yayında mı, Vitrinde mi, Kampanyalı – green/red checks)
   - Images column
5. Pagination + sorting + filtering built into GridMvc

Other page types that exist:
- Dashboard
- Create/Edit forms (SaveOrEdit)
- Media / File management
- Reports
- Settings forms
- Tree views + move products between categories
- Coupons, Orders, Customers, Menus, Tags, FAQ, Mail Templates, etc.

TECHNICAL CONSTRAINTS:
- Must remain compatible with Bootstrap 3 classes (btn, panel, form-control, glyphicon/fa icons, grid system)
- GridMvc structure must stay (table.grid-table)
- Turkish language throughout
- Current CSS variables and class names (admin-sidebar, admin-topbar, admin-content, admin-grid-ops, etc.) can be extended or refined

DESIGN GOAL – Modern 2025 SaaS Admin (Shopify / Linear / Vercel / Stripe Dashboard level):

Overall visual language:
- Clean, calm, professional, high-end
- Soft shadows, 8–12px border-radius
- Consistent spacing system
- Excellent visual hierarchy
- Dense but comfortable data tables (row height ~48–56px)
- Soft horizontal dividers only (no heavy borders)
- Muted secondary text
- Primary accent color (keep current blue #62b0e8 or refine it)
- Status colors: soft green / orange / red / gray pills instead of large green/red check icons
- Modern typography (Inter or system-ui stack)

Specific component upgrades:

1. Sidebar
- Keep dark slate navy
- Smoother hover/active states
- Better spacing and icon alignment
- Clearer group toggles

2. Top bar
- Cleaner, slightly taller if needed
- Better visual separation of actions

3. Bulk Toolbar
- Convert the current multi-colored button row into a clean modern toolbar
- Primary “+ Yeni Ürün / Yeni Kayıt” stands out
- Search always visible
- Bulk actions appear in a sticky/floating selection bar when rows are checked
- Secondary actions more subtle or in a “⋯ Daha fazla” dropdown

4. Data Tables (most important)
- Product thumbnail (48×48 rounded) + Name (bold) + secondary line
- Price: current price bold + strikethrough original + small “%X indirim” pill
- Status columns → soft colored pills/badges
- Position remains editable input
- Action icons appear on hover or in a clean overflow menu
- Subtle row hover + selected row left accent border
- Clean pagination

5. Forms & other pages
- Consistent modern form styling (labels, inputs, buttons)
- Card-based content areas instead of heavy Bootstrap panels
- Better empty states and “Resim Yok” placeholders

Deliverables:
- Full high-fidelity redesign of the admin shell (sidebar + topbar + content)
- Detailed redesign of a typical Product listing page (the most complex one)
- Consistent design system that can be applied to all other admin pages (Orders, Customers, Categories, Coupons, Stories, Settings, Dashboard, etc.)
- Desktop-first, then show how it adapts on tablet/mobile
- Keep all existing Turkish text and functionality

Style references: modern SaaS admin panels 2024–2026, clean Bootstrap 3 upgrades, Shopify admin, Linear, Vercel dashboard.
