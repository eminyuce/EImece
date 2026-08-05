using System.Data;
using System.Data.Common;
using EImece.Domain.Core.Admin;
using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using EImece.Domain.Core.Enums;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class AjaxController : BaseAdminController
{
    private readonly EImeceDbContext _db;
    private readonly IProductAdminService _products;
    private readonly IGridOrderingService _ordering;

    public AjaxController(
        IOptions<EImeceOptions> siteOptions,
        EImeceDbContext db,
        IProductAdminService products,
        IGridOrderingService ordering) : base(siteOptions)
    {
        _db = db;
        _products = products;
        _ordering = ordering;
    }

    // --- Product-specific ---

    [HttpPost]
    public Task<IActionResult> DeleteProductGridItem([FromBody] GridDeleteRequest request, CancellationToken cancellationToken)
        => SoftDeleteGrid(_db.Products, request, cancellationToken);

    [HttpPost]
    public async Task<IActionResult> ChangeProductGridOrderingOrState(
        [FromBody] ProductGridOrderingRequest request, CancellationToken cancellationToken)
    {
        request ??= new ProductGridOrderingRequest();
        await _products.ApplyOrderingOrStateAsync(request.Values, request.Checkbox, cancellationToken).ConfigureAwait(false);
        return Json(new { values = request.Values, checkbox = request.Checkbox });
    }

    [HttpPost]
    public async Task<IActionResult> ProductStateChanged(
        [FromBody] ProductStateChangeRequest request, CancellationToken cancellationToken)
    {
        request ??= new ProductStateChangeRequest();
        if (!Enum.IsDefined(typeof(ProductState), request.ProductStateSelection))
            return BadRequest(new { success = false, message = "Geçersiz ürün durumu" });

        await _products.ChangeProductStateAsync(request.Values, (ProductState)request.ProductStateSelection, cancellationToken)
            .ConfigureAwait(false);
        return Json(new { values = request.Values, ProductStateSelection = request.ProductStateSelection });
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePrices([FromBody] UpdatePriceRequest request, CancellationToken cancellationToken)
    {
        if (request?.PercentageOfIncreaseOrDecrease is null)
            return Json(new { success = false, message = "Yüzde değeri gerekli." });

        try
        {
            var affected = await ExecuteUpdatePricesAsync(request, cancellationToken).ConfigureAwait(false);
            return Json(new { success = true, affectedRows = affected });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Hata: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> GetProductDetailToolTip([FromBody] ProductIdRequest request, CancellationToken cancellationToken)
    {
        var id = request?.ProductId ?? 0;
        var p = await _db.Products.AsNoTracking()
            .Include(x => x.ProductCategory)
            .Include(x => x.Brand)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (p is null) return Json("<div class='text-muted'>Ürün bulunamadı</div>");

        var html = $@"<div class='small border rounded p-2 bg-light'>
<strong>{System.Net.WebUtility.HtmlEncode(p.Name)}</strong><br/>
Kod: {System.Net.WebUtility.HtmlEncode(p.ProductCode)}<br/>
Kategori: {System.Net.WebUtility.HtmlEncode(p.ProductCategory?.Name ?? "-")}<br/>
Marka: {System.Net.WebUtility.HtmlEncode(p.Brand?.Name ?? "-")}<br/>
Fiyat: {p.Price:N2} ₺ · Durum: {System.Net.WebUtility.HtmlEncode(p.State)}
</div>";
        return Json(html);
    }

    [HttpPost]
    public async Task<IActionResult> GetProductTags([FromBody] ProductTagsRequest request, CancellationToken cancellationToken)
    {
        var selected = await _db.ProductTags.AsNoTracking()
            .Where(t => t.ProductId == request.ProductId)
            .Select(t => t.TagId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return Json(await RenderTagCheckboxesAsync(selected, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost]
    public async Task<IActionResult> GetStoryTags([FromBody] StoryTagsRequest request, CancellationToken cancellationToken)
    {
        var selected = await _db.StoryTags.AsNoTracking()
            .Where(t => t.StoryId == request.StoryId)
            .Select(t => t.TagId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return Json(await RenderTagCheckboxesAsync(selected, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost]
    public async Task<IActionResult> GetTags(CancellationToken cancellationToken)
        => Json(await RenderTagCheckboxesAsync([], cancellationToken).ConfigureAwait(false));

    // --- Orders ---

    [HttpPost]
    public async Task<IActionResult> SaveAdminOrderNote([FromBody] SaveAdminOrderNoteRequest request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken).ConfigureAwait(false);
        if (order is null) return Json("Sipariş bulunamadı");
        order.AdminOrderNote = request.AdminOrderNote;
        order.ShipmentCompanyName = request.ShipmentCompanyName;
        order.ShipmentTrackingNumber = request.ShipmentTrackingNumber;
        order.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Json("Başarıyla kaydedildi");
    }

    [HttpPost]
    public async Task<IActionResult> ChangedOrderStatus([FromBody] ChangedOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken).ConfigureAwait(false);
        if (order is null) return Json("Sipariş bulunamadı");
        if (int.TryParse(request.OrderStatus, out var status))
            order.OrderStatus = status;
        order.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Json("Sipariş durumu güncellendi");
    }

    // --- Content image ---

    [HttpPost]
    public async Task<IActionResult> DeleteBaseContentMainImage([FromBody] DeleteMainImageRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ContentClass))
            return Json("Error contentClassName does not exists");

        if (request.ContentClass.Equals(nameof(Product), StringComparison.OrdinalIgnoreCase))
        {
            await ClearMainImageAsync(_db.Products, request.ContentId, request.ImageId, cancellationToken).ConfigureAwait(false);
        }
        else if (request.ContentClass.Equals(nameof(Menu), StringComparison.OrdinalIgnoreCase))
        {
            await ClearMainImageAsync(_db.Menus, request.ContentId, request.ImageId, cancellationToken).ConfigureAwait(false);
        }
        else if (request.ContentClass.Equals(nameof(ProductCategory), StringComparison.OrdinalIgnoreCase))
        {
            await ClearMainImageAsync(_db.ProductCategories, request.ContentId, request.ImageId, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            return Json("Unsupported content class");
        }

        return Json("<span class='text-muted'>Resim silindi</span>");
    }

    // --- Search autocomplete ---

    [HttpPost]
    public async Task<IActionResult> SearchAutoComplete([FromBody] SearchAutoCompleteRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Term) || string.IsNullOrWhiteSpace(request.Controller))
            return Json(Array.Empty<string>());

        var term = request.Term.Trim();
        var lang = SiteOptions.MainLanguage;
        var isIndex = string.Equals(request.Action, "Index", StringComparison.OrdinalIgnoreCase);
        if (!isIndex) return Json(Array.Empty<string>());

        List<string> list = request.Controller.ToLowerInvariant() switch
        {
            "products" => await _db.Products.AsNoTracking()
                .Where(r => r.Name.Contains(term) && (lang == 0 || r.Lang == lang))
                .OrderBy(r => r.Name).Select(r => r.Name).Take(15).ToListAsync(cancellationToken),
            "stories" => await _db.Stories.AsNoTracking()
                .Where(r => r.Name.Contains(term) && (lang == 0 || r.Lang == lang))
                .OrderBy(r => r.Name).Select(r => r.Name).Take(15).ToListAsync(cancellationToken),
            "productcategories" => await _db.ProductCategories.AsNoTracking()
                .Where(r => r.Name.Contains(term)).OrderBy(r => r.Name).Select(r => r.Name).Take(15).ToListAsync(cancellationToken),
            "storycategories" => await _db.StoryCategories.AsNoTracking()
                .Where(r => r.Name.Contains(term)).OrderBy(r => r.Name).Select(r => r.Name).Take(15).ToListAsync(cancellationToken),
            "menus" => await _db.Menus.AsNoTracking()
                .Where(r => r.Name.Contains(term)).OrderBy(r => r.Name).Select(r => r.Name).Take(15).ToListAsync(cancellationToken),
            "tags" => await _db.Tags.AsNoTracking()
                .Where(r => r.Name.Contains(term)).OrderBy(r => r.Name).Select(r => r.Name).Take(15).ToListAsync(cancellationToken),
            "coupons" => await _db.Coupons.AsNoTracking()
                .Where(r => r.Name.Contains(term)).OrderBy(r => r.Name).Select(r => r.Name).Take(15).ToListAsync(cancellationToken),
            "tagcategories" => await _db.TagCategories.AsNoTracking()
                .Where(r => r.Name.Contains(term)).OrderBy(r => r.Name).Select(r => r.Name).Take(15).ToListAsync(cancellationToken),
            "subscribers" => await _db.Subscribers.AsNoTracking()
                .Where(r => r.Name != null && r.Name.Contains(term)).OrderBy(r => r.Name).Select(r => r.Name!).Take(15).ToListAsync(cancellationToken),
            "settings" => await _db.Settings.AsNoTracking()
                .Where(r => r.Name.Contains(term)).OrderBy(r => r.Name).Select(r => r.Name).Take(15).ToListAsync(cancellationToken),
            "mainpageimages" => await _db.MainPageImages.AsNoTracking()
                .Where(r => r.Name.Contains(term)).OrderBy(r => r.Name).Select(r => r.Name).Take(15).ToListAsync(cancellationToken),
            "brands" => await _db.Brands.AsNoTracking()
                .Where(r => r.Name.Contains(term)).OrderBy(r => r.Name).Select(r => r.Name).Take(15).ToListAsync(cancellationToken),
            _ => []
        };

        return Json(list);
    }

    // --- Deletes ---

    [HttpPost] public Task<IActionResult> DeleteBrandGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.Brands, r, ct);
    [HttpPost] public Task<IActionResult> DeleteCouponsGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.Coupons, r, ct);
    [HttpPost] public Task<IActionResult> DeleteCouponGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.Coupons, r, ct);
    [HttpPost] public Task<IActionResult> DeleteMenusGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.Menus, r, ct);
    [HttpPost] public Task<IActionResult> DeleteStoryGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.Stories, r, ct);
    [HttpPost] public Task<IActionResult> DeleteStoryCategoryGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.StoryCategories, r, ct);
    [HttpPost] public Task<IActionResult> DeleteFaqGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.Faqs, r, ct);
    [HttpPost] public Task<IActionResult> DeleteTagGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.Tags, r, ct);
    [HttpPost] public Task<IActionResult> DeleteTagCategoriesGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.TagCategories, r, ct);
    [HttpPost] public Task<IActionResult> DeleteTemplateGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.Templates, r, ct);
    [HttpPost] public Task<IActionResult> DeleteProductCategoriesGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.ProductCategories, r, ct);
    [HttpPost] public Task<IActionResult> DeleteSettingGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.Settings, r, ct);
    [HttpPost] public Task<IActionResult> DeleteProductCommentGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => HardDeleteGrid(_db.ProductComments, r, ct);
    [HttpPost] public Task<IActionResult> DeleteMediaGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.FileStorages, r, ct);
    [HttpPost] public Task<IActionResult> DeleteMainPageImageGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.MainPageImages, r, ct);
    [HttpPost] public Task<IActionResult> DeleteSubscriberGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.Subscribers, r, ct);
    [HttpPost] public Task<IActionResult> DeleteShoppingCartGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.ShoppingCarts, r, ct);
    [HttpPost] public Task<IActionResult> DeleteMailTemplateGridItem([FromBody] GridDeleteRequest r, CancellationToken ct) => SoftDeleteGrid(_db.MailTemplates, r, ct);

    // --- Ordering / state ---

    [HttpPost] public Task<IActionResult> ChangeBrandGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.Brands, r, ct);
    [HttpPost] public Task<IActionResult> ChangeProductCategoriesGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.ProductCategories, r, ct);
    [HttpPost] public Task<IActionResult> ChangeStoryGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.Stories, r, ct);
    [HttpPost] public Task<IActionResult> ChangeStoryCategoryGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.StoryCategories, r, ct);
    [HttpPost] public Task<IActionResult> ChangeTagGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.Tags, r, ct);
    [HttpPost] public Task<IActionResult> ChangeTagCategoriesGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.TagCategories, r, ct);
    [HttpPost] public Task<IActionResult> ChangeCouponGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.Coupons, r, ct);
    [HttpPost] public Task<IActionResult> ChangeCouponsGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.Coupons, r, ct);
    [HttpPost] public Task<IActionResult> ChangeTemplateGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.Templates, r, ct);
    [HttpPost] public Task<IActionResult> ChangeMenusGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.Menus, r, ct);
    [HttpPost] public Task<IActionResult> ChangeFaqGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.Faqs, r, ct);
    [HttpPost] public Task<IActionResult> ChangeMediaGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.FileStorages, r, ct);
    [HttpPost] public Task<IActionResult> ChangeMainPageImageGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.MainPageImages, r, ct);
    [HttpPost] public Task<IActionResult> ChangeSubscriberGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.Subscribers, r, ct);
    [HttpPost] public Task<IActionResult> ChangeProductCommentGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.ProductComments, r, ct);
    [HttpPost] public Task<IActionResult> ChangeMailTemplateGridOrderingOrState([FromBody] ProductGridOrderingRequest r, CancellationToken ct) => ChangeOrdering(_db.MailTemplates, r, ct);

    private async Task<IActionResult> ChangeOrdering<T>(DbSet<T> set, ProductGridOrderingRequest? request, CancellationToken ct)
        where T : BaseEntity
    {
        request ??= new ProductGridOrderingRequest();
        await _ordering.ApplyAsync(set, request.Values, request.Checkbox, ct).ConfigureAwait(false);
        return Json(new { values = request.Values, checkbox = request.Checkbox });
    }

    private async Task<IActionResult> SoftDeleteGrid<T>(DbSet<T> set, GridDeleteRequest request, CancellationToken cancellationToken)
        where T : BaseEntity
    {
        request ??= new GridDeleteRequest();
        var ids = ParseIds(request.Values);
        var entities = await set.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var entity in entities)
        {
            entity.IsActive = false;
            entity.UpdatedDate = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        // Legacy adminEimece.deleteItemsSuccess expects a raw array.
        return Json(request.Values);
    }

    private async Task<IActionResult> HardDeleteGrid<T>(DbSet<T> set, GridDeleteRequest request, CancellationToken cancellationToken)
        where T : class, IEntity<int>
    {
        request ??= new GridDeleteRequest();
        var ids = ParseIds(request.Values);
        var entities = await set.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken).ConfigureAwait(false);
        set.RemoveRange(entities);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Json(request.Values);
    }

    private async Task ClearMainImageAsync<T>(DbSet<T> set, int contentId, int imageId, CancellationToken ct)
        where T : BaseContent
    {
        var entity = await set.FirstOrDefaultAsync(e => e.Id == contentId, ct).ConfigureAwait(false);
        if (entity is null) return;
        entity.MainImageId = null;
        entity.UpdatedDate = DateTime.UtcNow;
        var file = await _db.FileStorages.FirstOrDefaultAsync(f => f.Id == imageId, ct).ConfigureAwait(false);
        if (file is not null)
        {
            file.IsActive = false;
            file.UpdatedDate = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private async Task<string> RenderTagCheckboxesAsync(IReadOnlyList<int> selected, CancellationToken ct)
    {
        var cats = await _db.TagCategories.AsNoTracking()
            .Include(c => c.Tags)
            .OrderBy(c => c.Position).ThenBy(c => c.Name)
            .ToListAsync(ct).ConfigureAwait(false);
        if (cats.Count == 0) return "";

        var sb = new System.Text.StringBuilder();
        foreach (var cat in cats)
        {
            sb.Append("<h6>").Append(System.Net.WebUtility.HtmlEncode(cat.Name)).Append("</h6><div class='row g-1 mb-2'>");
            foreach (var tag in cat.Tags.OrderBy(t => t.Name))
            {
                var check = selected.Contains(tag.Id) ? " checked" : "";
                sb.Append("<div class='col-md-3'><label class='form-check-label'><input class='form-check-input' type='checkbox' name='tags' value='")
                    .Append(tag.Id).Append('\'').Append(check).Append(" /> ")
                    .Append(System.Net.WebUtility.HtmlEncode(tag.Name)).Append("</label></div>");
            }
            sb.Append("</div>");
        }
        return sb.ToString();
    }

    private async Task<string> ExecuteUpdatePricesAsync(UpdatePriceRequest request, CancellationToken ct)
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct).ConfigureAwait(false);

        await using DbCommand cmd = conn.CreateCommand();
        cmd.CommandText = "[dbo].[UpdateProductPrices]";
        cmd.CommandType = CommandType.StoredProcedure;
        AddParam(cmd, "@PercentageOfIncreaseOrDecrease", request.PercentageOfIncreaseOrDecrease);
        AddParam(cmd, "@ProductId", (object?)request.ProductId ?? DBNull.Value);
        AddParam(cmd, "@CategoryId", (object?)request.CategoryId ?? DBNull.Value);
        AddParam(cmd, "@BrandId", (object?)request.BrandId ?? DBNull.Value);
        AddParam(cmd, "@TagId", (object?)request.TagId ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return result?.ToString() ?? "0";
    }

    private static void AddParam(DbCommand cmd, string name, object? value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(p);
    }

    private static List<int> ParseIds(IReadOnlyList<string> values)
    {
        var ids = new List<int>();
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value)) continue;
            var part = value.Split('-')[0];
            if (int.TryParse(part, out var id)) ids.Add(id);
        }
        return ids;
    }
}
