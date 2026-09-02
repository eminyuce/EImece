# Modernize legacy admin panels

- **Captured:** 2026-08-10 10:05:22 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

You are an expert ASP.NET MVC frontend engineer specializing in modernizing legacy admin panels while keeping the existing data-grid library and all current business functionalities.

### Critical Constraints
1. The project uses the NuGet package *Grid.Mvc* (~3.0.0). 
   We must NOT replace it with any other grid library. 
   All improvements must be built on top of Grid.Mvc (custom columns, RenderValueAs, custom filter widgets, CSS, extra JavaScript, partial views).

2. Do NOT remove or break existing interactive functionalities that already exist in the current UI.
   Especially important:
   - The green/red status icons (Yayında mı?, Vitrinde mi?, Kampanyalı, etc.) are clickable and toggle the status of the product.
   - These toggle actions must remain fully functional.
   - Any other existing quick-actions (edit, image management, order/position change, bulk status updates, etc.) must also be preserved.

### Goal
Create the best possible modern data-grid experience across the entire Erayweb Yönetim Paneli while:
- Staying 100% on Grid.Mvc
- Keeping every existing interactive status toggle and quick action working
- Making the UI much cleaner, more scannable, and consistent on every list page (Products, Orders, Customers, Categories, Brands, Coupons, Blogs, etc.)

### Design & UX Goals (while preserving functionality)

1. Visual Hierarchy
   - Single clean product/order name as a strong link + muted secondary info (code, ID) underneath
   - Remove duplicated text blocks
   - Statuses shown as modern, compact, colored badges/chips
   - The clickable green/red status icons must still be present and fully working (they can be redesigned as nicer toggle switches, icon buttons, or interactive badges, but the click behavior and backend calls must stay exactly the same)

2. Image Column
   - Always show a small thumbnail or consistent placeholder
   - Keep any existing “Resmi Göster / Resmi Sil” functionality

3. Actions
   - Move the small scattered icons (camera, edit, +, delete, etc.) into a clean Actions dropdown or a compact button group on the right
   - Do not lose any of the current actions

4. Status Toggles (Critical)
   - Redesign the current green ✓ / red ✗ icons into a cleaner interactive control (nice toggle switches, pill buttons, or icon buttons with clear hover/active states)
   - They must remain individually clickable and perform the exact same status change as today
   - Group related statuses nicely (Yayında / Vitrinde / Kampanyalı) so the row does not look noisy

5. Other Required Improvements
   - Sticky header + sticky first column + sticky Actions column
   - Density toggle (Comfortable / Compact)
   - Global search bar + quick filter chips above the grid
   - Better bulk action bar
   - Modern styling of Grid.Mvc filters and pager
   - Consistent empty/loading states
   - Soft row hover and selected-row highlighting

### Technical Approach
- Heavily restyle Grid.Mvc via CSS
- Use custom columns (RenderValueAs) extensively for name, status toggles, images, price, and actions
- Keep the existing JavaScript/AJAX calls that power the status toggles — only change the visual trigger elements
- Create reusable helpers/partials so the same patterns can be applied to every list page
- Progressive enhancement only

### Deliverables
1. Architecture for shared grid enhancements (CSS, helpers, partials, JS)
2. New modern CSS for Grid.Mvc tables
3. Reusable column helpers, especially for:
   - Name + meta
   - Interactive status toggles (preserving current functionality)
   - Image thumbnail + existing image actions
   - Price with discount
   - Actions dropdown
4. Full reference implementation for the Products page
5. Clear patterns that can be reused on all other list pages

Start by confirming how the current status icons work (they are clickable toggles), then propose the exact visual redesign of those toggles that keeps the functionality 100% intact. After that, implement the complete Products page reference.
