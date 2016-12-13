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
    public interface IEImeceContext : IDisposable, IEntitiesContext
    {
        DbSet<Product> Products { get; set; }
        DbSet<ProductFile> ProductFiles { get; set; }
        DbSet<ProductCategory> ProductCategories { get; set; }
        DbSet<Menu> Menus { get; set; }
    }
}
