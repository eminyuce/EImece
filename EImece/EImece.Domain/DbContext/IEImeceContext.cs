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
    }
}
