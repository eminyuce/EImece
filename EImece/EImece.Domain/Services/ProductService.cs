﻿using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using Ninject;

namespace EImece.Domain.Services
{
    public class ProductService : BaseContentService<Product>, IProductService
    {

        private IProductRepository ProductRepository { get; set; }
        public ProductService(IProductRepository repository) : base(repository)
        {
            ProductRepository = repository;
        }

        public List<Product> GetAdminPageList(int id, string search, int lang)
        {
            return ProductRepository.GetAdminPageList(id, search, lang);
        }

        public List<Product> GetMainPageProducts(int page, int lang)
        {
            return ProductRepository.GetMainPageProducts(page, lang);
        }

         
        public void SaveProductTags(int id, int[] tags)
        {
            ProductTagRepository.SaveProductTags(id, tags);
        }

        public List<ProductTag> GetProductTagsByProductId(int productId)
        {
            return ProductTagRepository.GetAllByProductId(productId);
        }
    }
}