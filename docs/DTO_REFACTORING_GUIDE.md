# Razor Pages DTO Refactoring Guide

This guide shows how to decouple Razor Pages from Entity Framework entities by introducing a DTO-based application layer.

## 1) Suggested folder structure

```text
EImece/
├─ EImece.Domain/
│  ├─ Entities/
│  │  ├─ Product.cs
│  │  ├─ Category.cs
│  │  ├─ Order.cs
│  │  ├─ OrderItem.cs
│  │  └─ Customer.cs
│  └─ ...
├─ EImece.Application/
│  ├─ DTOs/
│  │  ├─ Product/
│  │  │  ├─ ProductListDto.cs
│  │  │  ├─ ProductDetailDto.cs
│  │  │  ├─ ProductCreateDto.cs
│  │  │  └─ ProductUpdateDto.cs
│  │  ├─ Category/
│  │  │  ├─ CategoryListDto.cs
│  │  │  └─ CategoryDetailDto.cs
│  │  ├─ Order/
│  │  │  ├─ OrderListDto.cs
│  │  │  ├─ OrderDetailDto.cs
│  │  │  └─ OrderCreateDto.cs
│  │  └─ Customer/
│  │     ├─ CustomerListDto.cs
│  │     └─ CustomerDetailDto.cs
│  ├─ Interfaces/
│  │  ├─ IProductService.cs
│  │  ├─ ICategoryService.cs
│  │  ├─ IOrderService.cs
│  │  └─ ICustomerService.cs
│  ├─ Services/
│  │  ├─ ProductService.cs
│  │  ├─ CategoryService.cs
│  │  ├─ OrderService.cs
│  │  └─ CustomerService.cs
│  └─ Mapping/
│     └─ DtoMappingProfile.cs
└─ EImece.Web/
   ├─ Pages/
   │  ├─ Products/
   │  │  ├─ Index.cshtml
   │  │  ├─ Index.cshtml.cs
   │  │  ├─ Edit.cshtml
   │  │  └─ Edit.cshtml.cs
   │  └─ Orders/
   └─ ...
```

> If you don't want to create a separate `EImece.Application` project immediately, start with `EImece/DTOs`, `EImece/Services`, and `EImece/Mapping` folders in the web project, then split later.

## 2) DTO examples (list, detail, create/update)

### Product list DTO

```csharp
public sealed class ProductListDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public bool IsActive { get; init; }
    public string CategoryName { get; init; } = string.Empty;
}
```

### Product detail DTO

```csharp
public sealed class ProductDetailDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public bool IsActive { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
}
```

### Product create DTO

```csharp
public sealed class ProductCreateDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    [StringLength(64)]
    public string Sku { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "99999999")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Required]
    public int CategoryId { get; set; }
}
```

### Product update DTO

```csharp
public sealed class ProductUpdateDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "99999999")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }

    [Required]
    public int CategoryId { get; set; }
}
```

## 3) Service-layer mapping examples

### Interface example

```csharp
public interface IProductService
{
    Task<IReadOnlyList<ProductListDto>> GetProductsAsync(CancellationToken ct = default);
    Task<ProductDetailDto?> GetProductByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateProductAsync(ProductCreateDto dto, CancellationToken ct = default);
    Task UpdateProductAsync(ProductUpdateDto dto, CancellationToken ct = default);
}
```

### AutoMapper style

```csharp
public sealed class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        return entity is null ? null : _mapper.Map<ProductDetailDto>(entity);
    }

    public async Task<int> CreateProductAsync(ProductCreateDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<Product>(dto);
        await _repository.AddAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task UpdateProductAsync(ProductUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, ct)
            ?? throw new InvalidOperationException($"Product not found: {dto.Id}");

        _mapper.Map(dto, entity);

        await _repository.SaveChangesAsync(ct);
    }
}
```

### Manual mapping style

```csharp
public sealed class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
        {
            return null;
        }

        return new ProductDetailDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Sku = entity.Sku,
            Price = entity.Price,
            StockQuantity = entity.StockQuantity,
            IsActive = entity.IsActive,
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category?.Name ?? string.Empty
        };
    }

    public async Task<int> CreateProductAsync(ProductCreateDto dto, CancellationToken ct = default)
    {
        var entity = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Sku = dto.Sku,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            IsActive = true
        };

        await _repository.AddAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task UpdateProductAsync(ProductUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(dto.Id, ct)
            ?? throw new InvalidOperationException($"Product not found: {dto.Id}");

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Price = dto.Price;
        entity.StockQuantity = dto.StockQuantity;
        entity.IsActive = dto.IsActive;
        entity.CategoryId = dto.CategoryId;

        await _repository.SaveChangesAsync(ct);
    }
}
```

### AutoMapper profile example

```csharp
public sealed class DtoMappingProfile : Profile
{
    public DtoMappingProfile()
    {
        CreateMap<Product, ProductListDto>()
            .ForMember(d => d.CategoryName,
                o => o.MapFrom(s => s.Category.Name));

        CreateMap<Product, ProductDetailDto>()
            .ForMember(d => d.CategoryName,
                o => o.MapFrom(s => s.Category.Name));

        CreateMap<ProductCreateDto, Product>();

        CreateMap<ProductUpdateDto, Product>()
            .ForMember(d => d.Id, o => o.Ignore());
    }
}
```

## 4) Razor Page model before vs after

### Before (tight coupling)

```csharp
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    [BindProperty]
    public Product Product { get; set; } = default!;

    public EditModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task OnGetAsync(int id)
    {
        Product = await _db.Products.FirstAsync(x => x.Id == id);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _db.Products.Update(Product);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
```

### After (DTO + service only)

```csharp
using EImece.Application.DTOs.Product;
using EImece.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class EditModel : PageModel
{
    private readonly IProductService _productService;

    [BindProperty]
    public ProductUpdateDto Product { get; set; } = new();

    public EditModel(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var detail = await _productService.GetProductByIdAsync(id);
        if (detail is null)
        {
            return NotFound();
        }

        Product = new ProductUpdateDto
        {
            Id = detail.Id,
            Name = detail.Name,
            Description = detail.Description,
            Price = detail.Price,
            StockQuantity = detail.StockQuantity,
            IsActive = detail.IsActive,
            CategoryId = detail.CategoryId
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _productService.UpdateProductAsync(Product);
        return RedirectToPage("Index");
    }
}
```

## 5) Recommended rollout plan (for Product, Category, Order, OrderItem, Customer)

1. Create DTO folders and base service interfaces per aggregate.
2. Move read operations first (`Index`, `Details`) to DTO-returning services.
3. Move create/update forms to `CreateDto` / `UpdateDto`.
4. Remove direct `DbContext` usage from pages.
5. Remove `using ...Domain.Entities` from page models.
6. Add tests around mapping and service behavior.
7. Repeat for each aggregate: Product, Category, Customer, Order, OrderItem.

## 6) Best practices and pitfalls

### Best practices

- Keep DTOs use-case specific (list/detail/create/update), not one giant DTO.
- Validate input DTOs with data annotations and/or FluentValidation.
- Keep mapping close to services (AutoMapper profiles in Application layer).
- Use async all the way (`Task`, `CancellationToken`).
- Return thin DTOs to UI to avoid over-fetching and accidental data leaks.
- Use projection (`Select`) for list endpoints to reduce query cost.

### Common pitfalls

- Reusing entity classes as view models “temporarily” and never cleaning up.
- Returning entities with lazy-loaded navigation properties to UI.
- Putting mapping logic in page models (violates separation of concerns).
- Exposing sensitive fields in DTOs (`CostPrice`, internal flags, audit columns).
- Creating circular DTO graphs (`Order -> Customer -> Orders -> ...`).
- Doing partial updates without concurrency checks (row version / timestamp).

## 7) Definition of done checklist

- No page model references `...Domain.Entities`.
- No page model injects `DbContext`.
- All page model bind properties are DTOs.
- Services return DTOs for reads and accept DTOs for writes.
- Entity/DTO mapping exists only in the service layer (or dedicated mapper inside Application layer).
- Mapping and service tests pass.
