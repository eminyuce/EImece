using ClosedXML.Excel;
using EImece.Domain.Core.Admin;
using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class ProductsController : BaseAdminController
{
    private readonly EImeceDbContext _db;
    private readonly IProductAdminService _products;

    public ProductsController(
        IOptions<EImeceOptions> siteOptions,
        EImeceDbContext db,
        IProductAdminService products)
        : base(siteOptions)
    {
        _db = db;
        _products = products;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int id = 0,
        int brandId = 0,
        string? search = null,
        int page = 1,
        int pageSize = 25,
        string? sort = null,
        string? sortDir = null,
        CancellationToken cancellationToken = default)
    {
        var lang = SiteOptions.MainLanguage;
        var tree = await _products.BuildCategoryTreeAsync(lang, cancellationToken).ConfigureAwait(false);
        var (items, total) = await _products.GetProductsAsync(
            id, brandId, search, page, pageSize, sort, sortDir ?? "desc", lang, cancellationToken).ConfigureAwait(false);

        string? selectedName = null;
        if (id > 0)
        {
            selectedName = await _db.ProductCategories.AsNoTracking()
                .Where(c => c.Id == id).Select(c => c.Name).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        var brands = await _db.Brands.AsNoTracking()
            .Where(b => b.IsActive && (b.Lang == lang || lang == 0))
            .OrderBy(b => b.Position).ThenBy(b => b.Name)
            .Select(b => new BrandFilterItem { Id = b.Id, Name = b.Name })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return View(new ProductsAdminViewModel
        {
            Search = search,
            CategoryId = id,
            BrandId = brandId,
            SelectedCategoryName = selectedName,
            Page = Math.Max(1, page),
            PageSize = Math.Clamp(pageSize, 1, 200),
            TotalCount = total,
            Sort = sort,
            SortDir = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc",
            CategoryTree = tree,
            Brands = brands,
            Products = items
        });
    }

    [HttpGet]
    public async Task<IActionResult> SaveOrEdit(int id = 0, CancellationToken cancellationToken = default)
    {
        AdminEditViewModel model;
        if (id <= 0)
        {
            model = new AdminEditViewModel
            {
                Title = "Ürün — Yeni Giriş",
                ControllerName = "Products",
                EditProfile = "product",
                IsActive = true,
                ProductCode = "NEW-" + DateTime.UtcNow.Ticks
            };
        }
        else
        {
            var entity = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
            if (entity is null) return NotFound();
            model = new AdminEditViewModel
            {
                Title = "Ürün — Düzenle",
                ControllerName = "Products",
                EditProfile = "product",
                Id = entity.Id,
                Name = entity.Name,
                IsActive = entity.IsActive,
                Position = entity.Position,
                ProductCode = entity.ProductCode,
                Price = entity.Price,
                Discount = entity.Discount,
                ShortDescription = entity.ShortDescription,
                Description = entity.Description,
                ProductCategoryId = entity.ProductCategoryId,
                BrandId = entity.BrandId,
                MainPage = entity.MainPage,
                IsCampaign = entity.IsCampaign,
                MainImageId = entity.MainImageId
            };
        }

        await PopulateProductLookupsAsync(model, cancellationToken).ConfigureAwait(false);
        return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOrEdit(AdminEditViewModel model, CancellationToken cancellationToken)
    {
        model.EditProfile = "product";
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Ad zorunludur");
            await PopulateProductLookupsAsync(model, cancellationToken).ConfigureAwait(false);
            return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", model);
        }

        if (model.Id > 0)
        {
            var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == model.Id, cancellationToken).ConfigureAwait(false);
            if (entity is null) return NotFound();
            ApplyProductFields(entity, model);
            entity.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            var categoryId = model.ProductCategoryId
                ?? await _db.ProductCategories.AsNoTracking().Select(c => c.Id).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            var entity = new Product
            {
                Name = model.Name.Trim(),
                ProductCategoryId = categoryId,
                ProductCode = string.IsNullOrWhiteSpace(model.ProductCode) ? "NEW-" + DateTime.UtcNow.Ticks : model.ProductCode.Trim(),
                State = "ProductInStock",
                CreatedDate = DateTime.UtcNow,
                Lang = SiteOptions.MainLanguage
            };
            ApplyProductFields(entity, model);
            entity.UpdatedDate = DateTime.UtcNow;
            _db.Products.Add(entity);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Kaydedildi");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await _products.SoftDeleteAsync(new[] { id }, cancellationToken).ConfigureAwait(false);
        SetTempStatus("Ürün pasifleştirildi");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Media(int id)
        => RedirectToAction("Index", "Media", new { contentId = id, mod = "Products", imageType = "ProductGallery" });

    [HttpGet]
    public async Task<IActionResult> ExportExcel(CancellationToken cancellationToken)
    {
        var (items, _) = await _products.GetProductsAsync(0, 0, null, 1, 5000, null, "desc", SiteOptions.MainLanguage, cancellationToken)
            .ConfigureAwait(false);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Products");
        sheet.Cell(1, 1).Value = "Id";
        sheet.Cell(1, 2).Value = "Name";
        sheet.Cell(1, 3).Value = "Category";
        sheet.Cell(1, 4).Value = "Brand";
        sheet.Cell(1, 5).Value = "Price";
        sheet.Cell(1, 6).Value = "Discount";
        sheet.Cell(1, 7).Value = "ProductCode";
        sheet.Cell(1, 8).Value = "State";
        sheet.Cell(1, 9).Value = "IsActive";
        sheet.Cell(1, 10).Value = "MainPage";
        sheet.Cell(1, 11).Value = "IsCampaign";
        sheet.Cell(1, 12).Value = "Position";
        var row = 2;
        foreach (var p in items)
        {
            sheet.Cell(row, 1).Value = p.Id;
            sheet.Cell(row, 2).Value = p.Name;
            sheet.Cell(row, 3).Value = p.CategoryName;
            sheet.Cell(row, 4).Value = p.BrandName;
            sheet.Cell(row, 5).Value = p.Price;
            sheet.Cell(row, 6).Value = p.Discount;
            sheet.Cell(row, 7).Value = p.ProductCode;
            sheet.Cell(row, 8).Value = p.StateLabel;
            sheet.Cell(row, 9).Value = p.IsActive;
            sheet.Cell(row, 10).Value = p.MainPage;
            sheet.Cell(row, 11).Value = p.IsCampaign;
            sheet.Cell(row, 12).Value = p.Position;
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Products-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcelAsync(CancellationToken cancellationToken)
        => await ExportExcel(cancellationToken).ConfigureAwait(false);

    [HttpGet]
    public async Task<IActionResult> MoveProductsInTrees(int id = 0, string? productIdList = null, int oldCategoryId = 0, CancellationToken cancellationToken = default)
    {
        var tree = await _products.BuildCategoryTreeAsync(SiteOptions.MainLanguage, cancellationToken).ConfigureAwait(false);
        var (items, _) = id > 0
            ? await _products.GetProductsAsync(id, 0, null, 1, 500, null, "desc", SiteOptions.MainLanguage, cancellationToken).ConfigureAwait(false)
            : ([], 0);

        string? message = null;
        if (id > 0 && oldCategoryId > 0 && !string.IsNullOrWhiteSpace(productIdList))
        {
            var oldName = await _db.ProductCategories.AsNoTracking().Where(c => c.Id == oldCategoryId).Select(c => c.Name).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            var newName = await _db.ProductCategories.AsNoTracking().Where(c => c.Id == id).Select(c => c.Name).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            var count = productIdList.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
            message = $"Seçilen {count} ürün '{oldName}' kategorisinden '{newName}' kategorisine taşındı.";
        }

        return View(new MoveProductsViewModel
        {
            CategoryId = id,
            ProductIdList = productIdList,
            OldCategoryId = oldCategoryId,
            Message = message,
            CategoryTree = tree,
            Products = items
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveProducts(int id, string productIdList, int oldCategoryId, CancellationToken cancellationToken)
    {
        var ids = (productIdList ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
            .Where(n => n > 0);
        await _products.MoveProductsAsync(id, ids, cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(MoveProductsInTrees), new { id, productIdList, oldCategoryId });
    }

    private static void ApplyProductFields(Product entity, AdminEditViewModel model)
    {
        entity.Name = model.Name.Trim();
        entity.IsActive = model.IsActive;
        entity.Position = model.Position;
        entity.Price = model.Price;
        entity.Discount = model.Discount;
        entity.ShortDescription = model.ShortDescription;
        entity.Description = model.Description;
        entity.MainPage = model.MainPage;
        entity.IsCampaign = model.IsCampaign;
        if (!string.IsNullOrWhiteSpace(model.ProductCode)) entity.ProductCode = model.ProductCode.Trim();
        if (model.ProductCategoryId.HasValue) entity.ProductCategoryId = model.ProductCategoryId.Value;
        entity.BrandId = model.BrandId;
    }

    private async Task PopulateProductLookupsAsync(AdminEditViewModel model, CancellationToken cancellationToken)
    {
        model.ProductCategories = await _db.ProductCategories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.ProductCategoryId))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        model.Brands = await _db.Brands.AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.BrandId))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
