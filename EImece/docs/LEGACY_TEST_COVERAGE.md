# Legacy EImece Test Coverage

Safety net for the ASP.NET MVC 5 / .NET Framework 4.8.1 application used as the behavioral reference during the ASP.NET Core migration.

## Projects

| Project | Role |
|---------|------|
| `EImece.Tests` | Fast **unit** tests (MSTest + Moq). Live-SQL `HomeControllerTest` is `[Ignore]`d. |
| `EImece.Integration.Tests` | LocalDB `EImece_Legacy_Test` service/Ajax/auth/report integration tests. |

## How to run

```powershell
# Unit tests (VS / vstest)
msbuild EImece.Tests\EImece.Tests.csproj /t:Build /p:Configuration=Debug
vstest.console.exe EImece.Tests\bin\Debug\EImece.Tests.dll

# Integration tests (requires LocalDB)
dotnet test EImece.Integration.Tests\EImece.Integration.Tests.csproj

# Helper script
.\scripts\run-legacy-coverage.ps1
```

Set `EIMECE_DB_CONNECTION_STRING` only if you override the LocalDB catalog. Integration tests **never** target production `yuva8905_yuvadan` by default.

## Covered (meaningful critical scope)

### Unit (`EImece.Tests/Unit`)
- **Cart/coupon math:** `ShoppingCartSession.CalculateCouponDiscount`, totals
- **Grid ordering/state:** `BrandService.ChangeGridBaseEntityOrderingOrState` (Position / IsActive / MainPage)
- **Product state:** `ProductService.ChangeProductState`
- **Coupon / cart / order services:** repository delegation, null guards, SaveOrEdit timestamps
- **Admin Ajax P0:** `DeleteProductGridItem`, `ChangeProductGridOrderingOrState`, `ProductStateChanged`, `UpdatePrices`, `SaveAdminOrderNote`, `ChangedOrderStatus`, `DeleteBaseContentMainImage` (mocked services)
- **Auth filter:** `DeleteAuthorizeAttribute` redirect vs allow
- **Reports:** `ReportService.ConvertDataTableToList`
- **Account:** `LoginViewModel` data annotations
- **Payment:** coupon service contract used by `ApplyCoupon`

### Integration (`EImece.Integration.Tests`)
- LocalDB seed (category/brand/product/order)
- Brand ordering/state persistence
- Product state persistence + delete side effects
- Admin Ajax `ChangeBrandGridOrderingOrState` round-trip
- Order note/status Ajax persistence
- Shopping cart save/load by OrderGuid
- Cart session discount math
- DeleteAuthorize role gate
- Report export shape helper

## Deferred / out of scope
- Razor views, `Scripts/*.js` / Grid.Mvc client
- Full Iyzico network calls (mock at service boundary)
- `UpdateProductPrices` stored procedure against LocalDB (needs SP deploy)
- Exhaustive Admin CRUD controllers beyond Ajax P0 patterns
- `EImece.MyConsole`, Core (`EImece.Web`) projects
- Selenium / browser UI

## Risks
- Controllers use Ninject property injection; unit tests set properties manually.
- LocalDB `DropCreateDatabaseAlways` may fail if EF model/SQL features are unsupported — tests then `Inconclusive`.
- `DeleteAuthorize` only checks `Administrator` role today (`UserRoleHelper.GetDeletedRoles`).
- Legacy `HomeControllerTest` still present but ignored; do not re-enable against production DB.

## Production code changes
None required for this suite. Tests exercise public services/controllers/attributes as-is.

## Coverage target
Aim for **~90% of critical Domain Services + Admin Ajax + cart/order/coupon helpers**, not whole-solution line coverage including views/scripts.
