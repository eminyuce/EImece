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

        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }


    }
}
