using EImece.Domain.Entities;
using GenericRepository.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.DbContext
{
    public interface IEImeceContext : IEntitiesContext
    {
        DbSet<ListItem> ListItems { get; set; }
        DbSet<List> Lists { get; set; }
        DbSet<Product> Products { get; set; }
        DbSet<ProductTag> ProductTags { get; set; }
        DbSet<ProductFile> ProductFiles { get; set; }
        DbSet<ProductCategory> ProductCategories { get; set; }
        DbSet<Menu> Menus { get; set; }
        DbSet<Tag> Tags { get; set; }
        DbSet<TagCategory> TagCategories { get; set; }
        DbSet<Subscriber> Subscribers { get; set; }
        DbSet<Story> Stories { get; set; }
        DbSet<StoryCategory> StoryCategories { get; set; }
        DbSet<StoryFile> StoryFiles { get; set; }
        DbSet<StoryTag> StoryTags { get; set; }
        DbSet<ProductSpecification> ProductSpecifications { get; set; }
        DbSet<FileStorage> FileStorages { get; set; }
        DbSet<FileStorageTag> FileStorageTags { get; set; }
        DbSet<Setting> Settings { get; set; }
        DbSet<Template> Templates { get; set; }
        DbSet<MenuFile> MenuFiles { get; set; }
    }
}
