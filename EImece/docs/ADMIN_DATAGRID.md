# Admin Data Grid (Grid.Mvc / MVCGrid replacement)

## Stack (Admin area)

| Asset | Version | Path |
|-------|---------|------|
| Bootstrap | **5.3.8** | `wwwroot/lib/bootstrap/` |
| jQuery | **4.0.0** | `wwwroot/lib/jquery/jquery.min.js` |
| Bootstrap Icons | **1.13.1** | `wwwroot/lib/bootstrap-icons/` |

## Custom grid features

Implemented in `EntityList.cshtml` + `admin-datagrid.js` / `admin-datagrid.css`:

- Server search, column sort, paging, page size
- Row **Düzenle** / **Sil**
- Bulk select + Ajax soft-delete (`Admin/Ajax/Delete*GridItem`)
- Active/inactive badges
- Excel/CSV export link when controller exposes export
- Client-side filter on current page

Legacy Grid.Mvc / `MVCGridHandler.axd` are **not** used on Core.

## Controllers using the grid

Products, Brands, Coupons, Categories, Menus, Stories, Tags, Templates, Faq, Lists, Settings, Orders, Customers, ShoppingCarts, Subscribers, MailTemplates, MainPageImages, StoryCategories, TagCategories, Users, ProductComments, Media.
