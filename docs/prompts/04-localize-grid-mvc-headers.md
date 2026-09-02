# Localize Grid.Mvc column headers

- **Captured:** 2026-08-06 2:16:13 PM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

Task: Localize admin Grid.Mvc column headers to Turkish and open a PR.

Context

ASP.NET MVC admin area: EImece/EImece/Areas/Admin/Views/**
Grids use @Html.Grid(...).Columns(... .Titled(...))
UI strings live in EImece/Resources/AdminResource.resx (Turkish-first; no en satellite)
Prefer .Titled(AdminResource.SomeKey) when a key exists; otherwise Turkish literals or add a new AdminResource key
Reproduce / verify

/admin/faq/ still shows English State and UpdatedDate
Audit all admin Index grids for English .Titled("...") or untitled columns that fall back to English property names
Required fixes

Faq (Areas/Admin/Views/Faq/Index.cshtml): "State" → AdminResource.State (Durum); "UpdatedDate" → AdminResource.UpdatedDate (Güncelleme Tarihi)
Lists / Templates: "Title" → AdminResource.Title (Başlık)
AdminResource.resx + Resource.resx: IsValues value IsValues → Değer Listesi mi? (update Designer comments if present)
ShoppingCarts: page title/h2 → AdminResource.ShoppingCarts; "Order GUID" → Sipariş GUID; "User ID" → Kullanıcı Id
AppLogs: title untitled EventLevel / EventMessage / InnerErrorMessage as Seviye / Mesaj / İç Hata Mesajı; accordion “Log Text/Grid View” → Turkish
Orders: replace ASCII/English headers with AdminResource where available (OrderNumber, OrderStatus, CreatedDate, PaymentStatus, ShipmentTrackingNumber, AdminOrderNote, CargoPrice, CargoCompany); title PaidPrice as Ödenen Tutar; page title → AdminResource.Orders
MailTemplates: "Aktif" / dates → AdminResource.IsActive / CreatedDate / UpdatedDate
Brands / ProductCategories / Tags: "Fiyat Guncelleme" → "Fiyat Güncelleme"
Git / PR

Create branch from latest master (e.g. cursor/admin-grid-turkish-headers)
Commit only these localization changes
Push and open a PR with summary + short test plan (check /admin/faq/ and the other grids above)
Constraints

Do not edit _publish/
Do not change grid behavior—only headers/labels/resources
Match existing AdminResource patterns; don’t invent English UI strings


Agent
