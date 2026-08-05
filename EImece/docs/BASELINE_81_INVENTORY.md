# Baseline inventory — http://localhost:81 (2026-08-05)

BypassAdminAuth: **true**. Source: `publish/baseline-81.csv`.

## Working (HTTP &lt; 400)

| Area | Paths |
|------|--------|
| Storefront | `/`, `/health`, `/Account/Login`, `/Account/Register`, `/Account/ForgotPassword`, `/Payment/CheckoutBillingDetails`, `/Payment/CargoTracking`, `/p/advancedsearchproducts`, `/Customers/`, `/Manage/`, `/UnderConstruction`, `/robots.txt`, `/sitemap.xml` |
| Admin | Dashboard, Products (+SaveOrEdit), ProductCategories, Brands, Templates, Lists, Coupons, Orders, Customers, ShoppingCarts, Report (+CouponUsage, PaymentMethod, SalesByDateRange, FinancialReport, ProductSummary), Menus, Stories, StoryCategories, Tags, TagCategories, Faq, Subscribers, MailTemplates, MainPageImages, Settings/WebSiteLogo, AdminSettings, Metrics, Users, FileUpload, AppLogs, ImportData |

## Broken on :81 (pre-existing)

| Path | Status | Notes |
|------|--------|--------|
| `/Payment/` | 500 | Cart index fails on legacy |
| `/p/arama/?q=test` | 400 | Search route validation |
| `/Admin/Media` | 500 | Media library |
| `/Admin/ProductComments` | 500 | Comments admin |
| `/Admin/Images` | 500 | Compress images |
| `/info/1` | 404 | Sample info id missing |

## Parity target for :82

Everything in **Working** must work on Core. Broken legacy items should work on Core where feasible (especially Payment cart, search, Media).
