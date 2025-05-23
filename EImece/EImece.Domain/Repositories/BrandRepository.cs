﻿using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EImece.Domain.Repositories
{
    public class BrandRepository : BaseContentRepository<Brand>, IBrandRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BrandRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<Brand> GetAdminPageList(string search, int lang)
        {
            Expression<Func<Brand, object>> includeProperty3 = r => r.MainImage;
            Expression<Func<Brand, object>>[] includeProperties = { includeProperty3 };
            var brands = GetAllIncluding(includeProperties).Where(r => r.Lang == lang);
            if (!String.IsNullOrEmpty(search))
            {
                brands = brands.Where(r => r.Name.Contains(search));
            }
            brands = brands.OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate);

            return brands.ToList();
        }

        public List<Brand> GetBrandsIfAnyProductExists(int lang)
        {
            // Including the Products for checking if any product is associated with the Brand.
            Expression<Func<Brand, object>> includeProperty = r => r.Products;
            var brandsWithProducts = GetAllIncluding(includeProperty)
                .Where(r => r.Lang == lang && r.Products.Any())  // Check if there are any products associated with the brand
                .OrderBy(r => r.Position)  // Optional: Order by position
                .ThenByDescending(r => r.UpdatedDate);  // Optional: Then order by updated date

            return brandsWithProducts.ToList();  // Return the list of brands with products
        }
    }
}