using EImece.Domain.Entities;
using GenericRepository.EntityFramework;
using System;
using System.Data.Entity;

namespace EImece.Domain.DbContext
{
    public class EImeceContext : EntitiesContext, IEImeceContext
    {
        public EImeceContext()
        {
        }

        public EImeceContext(String nameOrConnectionString) : base(nameOrConnectionString)
        {
            this.Database.CommandTimeout = int.MaxValue;
            this.Configuration.LazyLoadingEnabled = false;
        }

        public IDbSet<MailTemplate> MailTemplates { get; set; }
        public IDbSet<List> Lists { get; set; }
        public IDbSet<ListItem> ListItems { get; set; }
        public IDbSet<Menu> Menus { get; set; }
        public IDbSet<ProductCategory> ProductCategories { get; set; }
        public IDbSet<Product> Products { get; set; }
        public IDbSet<ProductFile> ProductFiles { get; set; }
        public IDbSet<Tag> Tags { get; set; }
        public IDbSet<TagCategory> TagCategories { get; set; }
        public IDbSet<Subscriber> Subscribers { get; set; }
        public IDbSet<Story> Stories { get; set; }
        public IDbSet<StoryCategory> StoryCategories { get; set; }
        public IDbSet<StoryFile> StoryFiles { get; set; }
        public IDbSet<StoryTag> StoryTags { get; set; }
        public IDbSet<ProductSpecification> ProductSpecifications { get; set; }
        public IDbSet<ProductTag> ProductTags { get; set; }
        public IDbSet<FileStorage> FileStorages { get; set; }
        public IDbSet<FileStorageTag> FileStorageTags { get; set; }
        public IDbSet<Setting> Settings { get; set; }
        public IDbSet<Template> Templates { get; set; }
        public IDbSet<MenuFile> MenuFiles { get; set; }
        public IDbSet<MainPageImage> MainPageImages { get; set; }
        public IDbSet<BrowserSubscriber> BrowserSubscribers { get; set; }
        public IDbSet<BrowserSubscription> BrowserSubscriptions { get; set; }
        public IDbSet<BrowserNotificationFeedBack> BrowserNotificationFeedBacks { get; set; }
        public IDbSet<BrowserNotification> BrowserNotifications { get; set; }
        public IDbSet<Customer> Customers { get; set; }
        public IDbSet<Address> Addresses { get; set; }
        public IDbSet<ShoppingCart> ShoppingCarts { get; set; }
        public IDbSet<Order> Orders { get; set; }
        public IDbSet<OrderProduct> OrderProducts { get; set; }
        public IDbSet<Faq> Faqs { get; set; }
        public IDbSet<ProductComment> ProductComments { get; set; }
        public IDbSet<Brand> Brands { get; set; }
    }
}