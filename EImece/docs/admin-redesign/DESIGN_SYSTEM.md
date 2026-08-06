# Admin Panel Modern Redesign — Design System

Bootstrap 3 + GridMvc compatible visual system for the EImece admin area.

## Tokens (`adminShell.css`)

| Token | Value | Use |
| --- | --- | --- |
| `--admin-primary` | `#3b9dd9` | Primary buttons / accents (refined from `#62b0e8`) |
| `--admin-sidebar-accent` | `#62b0e8` | Active nav indicator |
| `--admin-sidebar-bg` | `#0f172a` | Dark slate navy sidebar |
| `--admin-content-bg` | `#f1f5f9` | Page canvas |
| `--admin-radius` / `--admin-radius-lg` | `10px` / `12px` | Cards, panels, menus |
| `--admin-row-height` | `52px` | Dense table rows (~48–56px) |
| Font | Inter / system-ui | Body and UI chrome |

Status soft colors: green `#059669` / orange `#d97706` / red `#dc2626` with matching soft backgrounds.

## Shell

- **Sidebar** (`admin-sidebar`): 250px → 68px icon-only collapse on desktop; off-canvas drawer on `<992px`.
- **Topbar** (`admin-topbar`): sticky 52px, frosted white, actions as quiet icon+label chips.
- **Content** (`admin-content`): 16–20px padding, left-aligned page titles (no heavy title banners).

## Shared listing pattern

1. Page `h2` (Turkish resource text unchanged)
2. Optional category tree in `admin-listing-sidebar` card
3. `pGridOperations` toolbar:
   - Primary `+ Yeni …` button
   - Always-visible search
   - Select All / Deselect All
   - **Daha fazla** dropdown (ordering, Excel, CSV)
   - Sticky dark **selection bar** (`#adminSelectionBar`) when rows are checked — delete, state activate/deactivate, product state
4. GridMvc `table.grid-table` inside soft card

All existing button IDs (`SelectAll`, `DeleteAll`, `SetStateOnAll`, `OrderingAll`, …) are preserved for `adminEimece.js`.

## Product listing upgrades

- 48×48 rounded thumbnail + bold name + secondary `NameLong`
- Price: bold current + strikethrough original + `%X indirim` pill
- Status columns: soft pills via `.admin-status-item` + existing `gridActiveIcon` / `gridNotActiveIcon` (JS class swaps still work)
- Row actions fade in on hover
- Selected rows: soft blue fill + left accent border
- Empty media: dashed `Resim Yok` placeholder

## Applying to other modules

No per-page redesign required for Orders, Customers, Coupons, Stories, Settings, etc.:

- Shell + panel card styles apply globally under `body.admin-app`
- Any page using `pGridOperations` gets the modern toolbar + selection bar
- Optional: wrap category trees with `admin-listing-layout` / `admin-listing-sidebar` like Products
- Optional: wrap name cells with `admin-product-cell` pattern where thumbnails exist

## Files

| File | Role |
| --- | --- |
| `Content/adminShell.css` | Shell tokens, sidebar, topbar, content, panels, forms |
| `Content/adminModern.css` | Toolbar, selection bar, grids, product cells, status pills |
| `Areas/Admin/Views/Shared/pGridOperations.cshtml` | Modern toolbar markup |
| `Areas/Admin/Views/Products/Index.cshtml` | Reference listing implementation |
| `Scripts/adminEimece.js` | Selection bar show/hide + count |
| `docs/admin-redesign/preview.html` | Static high-fidelity preview |

## Responsive

- **Desktop**: full sidebar + dense table; selection bar sticky under topbar
- **Tablet (`≤991`)**: drawer sidebar; category tree collapses to scrollable card; search full-width
- **Mobile (`≤767`)**: primary CTA full-width; selection groups stack; row actions always visible
