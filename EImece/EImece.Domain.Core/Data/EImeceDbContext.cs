using EImece.Domain.Core.Data.Configurations;
using EImece.Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EImece.Domain.Core.Data;

/// <summary>
/// EF Core business context (parallel to legacy EF6 EImeceContext).
/// Maps to the existing SQL Server schema; do not call EnsureCreated against production.
/// </summary>
public class EImeceDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public EImeceDbContext(DbContextOptions<EImeceDbContext> options)
        : base(options)
    {
    }

    public DbSet<MailTemplate> MailTemplates => Set<MailTemplate>();
    public DbSet<Entities.List> Lists => Set<Entities.List>();
    public DbSet<ListItem> ListItems => Set<ListItem>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductFile> ProductFiles => Set<ProductFile>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TagCategory> TagCategories => Set<TagCategory>();
    public DbSet<Subscriber> Subscribers => Set<Subscriber>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<StoryCategory> StoryCategories => Set<StoryCategory>();
    public DbSet<StoryFile> StoryFiles => Set<StoryFile>();
    public DbSet<StoryTag> StoryTags => Set<StoryTag>();
    public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();
    public DbSet<FileStorage> FileStorages => Set<FileStorage>();
    public DbSet<FileStorageTag> FileStorageTags => Set<FileStorageTag>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<MenuFile> MenuFiles => Set<MenuFile>();
    public DbSet<MainPageImage> MainPageImages => Set<MainPageImage>();
    public DbSet<BrowserSubscriber> BrowserSubscribers => Set<BrowserSubscriber>();
    public DbSet<BrowserSubscription> BrowserSubscriptions => Set<BrowserSubscription>();
    public DbSet<BrowserNotificationFeedBack> BrowserNotificationFeedBacks => Set<BrowserNotificationFeedBack>();
    public DbSet<BrowserNotification> BrowserNotifications => Set<BrowserNotification>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderProduct> OrderProducts => Set<OrderProduct>();
    public DbSet<Faq> Faqs => Set<Faq>();
    public DbSet<ProductComment> ProductComments => Set<ProductComment>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());

        // Restrict cascades on join / child rows to avoid accidental multi-cascade paths on SQL Server.
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(e => e.GetForeignKeys())
                     .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
