using EImece.Domain.Entities;
using GenericRepository.EntityFramework;
using System.Data.Entity;

namespace EImece.Domain.DbContext
{
    public interface IEImeceContext : IEntitiesContext
    {
        IDbSet<MailTemplate> MailTemplates { get; set; }
        IDbSet<ListItem> ListItems { get; set; }
        IDbSet<List> Lists { get; set; }
        IDbSet<Product> Products { get; set; }
        IDbSet<ProductTag> ProductTags { get; set; }
        IDbSet<ProductFile> ProductFiles { get; set; }
        IDbSet<ProductCategory> ProductCategories { get; set; }
        IDbSet<Menu> Menus { get; set; }
        IDbSet<Tag> Tags { get; set; }
        IDbSet<TagCategory> TagCategories { get; set; }
        IDbSet<Subscriber> Subscribers { get; set; }
        IDbSet<Story> Stories { get; set; }
        IDbSet<StoryCategory> StoryCategories { get; set; }
        IDbSet<StoryFile> StoryFiles { get; set; }
        IDbSet<StoryTag> StoryTags { get; set; }
        IDbSet<ProductSpecification> ProductSpecifications { get; set; }
        IDbSet<FileStorage> FileStorages { get; set; }
        IDbSet<FileStorageTag> FileStorageTags { get; set; }
        IDbSet<Setting> Settings { get; set; }
        IDbSet<Template> Templates { get; set; }
        IDbSet<MenuFile> MenuFiles { get; set; }
        IDbSet<BrowserSubscriber> BrowserSubscribers { get; set; }
        IDbSet<BrowserSubscription> BrowserSubscriptions { get; set; }
        IDbSet<BrowserNotificationFeedBack> BrowserNotificationFeedBacks { get; set; }
        IDbSet<BrowserNotification> BrowserNotifications { get; set; }
        IDbSet<Customer> Customers { get; set; }
        IDbSet<Address> Addresses { get; set; }
        IDbSet<ShoppingCart> ShoppingCarts { get; set; }
        IDbSet<Order> Orders { get; set; }
        IDbSet<OrderProduct> OrderProducts { get; set; }
        IDbSet<Faq> Faqs { get; set; }
        IDbSet<ProductComment> ProductComments { get; set; }

        IDbSet<Brand> Brands { get; set; }
        public IDbSet<Coupon> Coupons { get; set; }
    }
}