﻿using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IProductRepository : IBaseContentRepository<Product>
    {
        PaginatedList<Product> GetActiveProducts(int pageIndex, int pageSize, int language);

        PaginatedList<Product> GetMainPageProducts(int pageIndex, int pageSize, int language);

        List<Product> GetAdminPageList(int categoryId, string search, int language);

        List<Product> GetAdminPageList(int categoryId, int brandId, string search, int language);

        Product GetProduct(int id);

        PaginatedList<Product> SearchProducts(int pageIndex, int pageSize, string search, int lang, SortingType sorting);

        IEnumerable<Product> GetData(out int totalRecords, string globalSearch, String name, int? limitOffset, int? limitRowCount, string orderBy, bool desc);

        List<Product> GetRelatedProducts(int[] tagIdList, int take, int lang, int excludedProductId);

        List<Product> GetActiveProducts(int? language);

        ProductsSearchResult GetProductsSearchResult(
   string search,
   string filters,
   int top,
   int skip,
   int language);

        List<Product> GetChildrenProducts(int[] childrenCategoryId);

        List<Product> GetRandomProductsByCategoryId(int productCategoryId, int take, int lang, int excludedProductId);
    }
}