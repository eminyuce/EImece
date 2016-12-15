using EImece.Domain.Entities;
using GenericRepository.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace EImece.Domain.DbContext
{
    public class EImeceContext : EntitiesContext, IEImeceContext
    {
        
        public EImeceContext(String nameOrConnectionString) : base(nameOrConnectionString)
        {
            this.Database.CommandTimeout = int.MaxValue;
        }

        public DbSet<Menu> Menus { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductFile> ProductFiles { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TagCategory> TagCategories { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<Story> Stories { get; set; }
        public DbSet<StoryCategory> StoryCategories { get; set; }
        public DbSet<StoryFile> StoryFiles { get; set; }
        public DbSet<StoryTag> StoryTags { get; set; }
        public DbSet<ProductSpecification> ProductSpecifications { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }
        public DbSet<FileStorage> FileStorages { get; set; }
        public DbSet<FileStorageTag> FileStorageTags { get; set; }
    }
}
