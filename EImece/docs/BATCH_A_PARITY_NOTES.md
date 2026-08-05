# Batch A–D Parity Notes (ASP.NET Core 8)

## What changed

- `EImece.Domain.Core`: `ReportService` (legacy stored procs), `ReportExportService` (ClosedXML/CsvHelper), `ShoppingCartService`
- `EImece.Web` Admin: full sidebar Index pages, Report hub + Excel/CSV export, SaveOrEdit/Delete for catalog entities, FileUpload, Media, Users, Settings
- Storefront: Home/Search/Category/Detail load real EF Core data; cart session + order persistence; Register/ForgotPassword
- Config: same SQL as `:81` (`yuva8905_yuvadan`), `BypassAdminAuth=true` for local smoke

## Known differences vs legacy `:81`

| Topic | Legacy | Core |
|-------|--------|------|
| Excel export | `.xls` / NPOI style | `.xlsx` ClosedXML |
| Admin grids | Grid.Mvc | Bootstrap tables (top 200) |
| Product create | Full form + images | Name/active/position (+ default category/code) |
| Payment | Full Iyzico cart UX | Session cart + order rows; Iyzico when keys set; demo success without keys |
| BypassAdminAuth | Web.config | appsettings + IIS env `EImece__BypassAdminAuth` |

## Manual verification checklist

1. `:81` Admin bypass — all sidebar Index + Reports 200  
2. `:82` `/health` — `Database=UP`  
3. `:82` Admin bypass — every sidebar link 200  
4. `:82` `/Admin/Report/CouponUsage` + Export xlsx/csv  
5. Storefront home products, `/p/arama/`, `/c/pc/{id}/`, product detail  
6. Add to cart → Checkout → PlaceOrder (demo or Iyzico)  
7. Register new customer  
8. Staging: set `BypassAdminAuth=false` and test AdminLogin  

## Cutover

Keep `:81` until checklist passes. Then point IIS site to `Eimece_Core` and set `BypassAdminAuth=false` in production.
