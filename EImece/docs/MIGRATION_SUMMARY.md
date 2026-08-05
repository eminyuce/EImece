# EImece — ASP.NET Core 8 Migration Summary

## Stack replacements

| Legacy | Core 8 |
|--------|--------|
| ASP.NET MVC 5.3 / .NET Framework 4.8.1 | ASP.NET Core MVC / **.NET 8 LTS** |
| System.Web + OWIN cookies | ASP.NET Core middleware + cookie auth |
| Microsoft.Extensions.DI (already) | Same DI, constructor injection |
| EF6 | **EF Core 8** (`EImeceDbContext` + `ApplicationDbContext`) |
| ASP.NET Identity 2 | **ASP.NET Core Identity** |
| `Web.config` | `appsettings.json` + env vars (`EIMECE_DB_CONNECTION_STRING`) |
| ImageProcessor / GDI | **SkiaSharp** |
| NPOI `.xls` reports | **ClosedXML** `.xlsx` + CsvHelper |
| RazorEngine emails | **Fluid** + MailKit |
| IIS site `Eimece` :81 | Parallel IIS site `Eimece_Core` :82 |

## Projects

- [`EImece.Web`](../EImece.Web/) — ASP.NET Core host (`Program.cs`)
- [`EImece.Domain.Core`](../EImece.Domain.Core/) — EF Core entities, cart, reports, media, payments, storefront services
- Legacy `EImece` + `EImece.Domain` remain for :81 until cutover

## Auth bypass (testing only)

- Legacy: `Web.config` `BypassAdminAuth=true`
- Core: `EImece:BypassAdminAuth` + IIS env `EImece__BypassAdminAuth=true` + `BypassAdminAuthMiddleware`
- **Production:** set `BypassAdminAuth=false` (`appsettings.Production.json`)

## Publish

```powershell
# Elevated
.\EImece\scripts\publish-core-iis82.ps1
```

Deploys to `C:\inetpub\wwwroot\Eimece_Core`, binding `*:82:`. Script stops app pool before copy, reasserts bypass env after robocopy, syncs legacy media into `wwwroot/media`.

## Culture

Default `tr-TR` via `EImece:ApplicationLanguages` and request localization cookies.

## Issues resolved during verification

| Issue | Resolution |
|-------|------------|
| IIS kept stale DLL after publish | Stop app pool + delete DLL before robocopy; verify DLL timestamp |
| Bypass lost after MIR deploy | Re-write `web.config` env vars **after** robocopy |
| AbsoluteRootPath to legacy media → 500 | App pool lacked write ACL; use copied media under Core wwwroot |
| `/Admin/ProductComments/` 404 | `id` required; made optional and list-all mode added |
| :81 cart/search/media broken | Core implements working replacements |

## Docs

- [BASELINE_81_INVENTORY.md](BASELINE_81_INVENTORY.md)
- [OLD_VS_NEW_VERIFICATION.md](OLD_VS_NEW_VERIFICATION.md)
- [FUNCTIONAL_VERIFICATION.md](FUNCTIONAL_VERIFICATION.md)
- [BATCH_A_PARITY_NOTES.md](BATCH_A_PARITY_NOTES.md)
