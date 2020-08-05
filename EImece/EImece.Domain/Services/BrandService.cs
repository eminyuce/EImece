using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Models;
using Microsoft.Owin.Security.OAuth;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Services
{
    public class BrandService : BaseContentService<Brand>, IBrandService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IBrandRepository BrandRepository;
        public BrandService(IBrandRepository repository) : base(repository)
        {
            BrandRepository = repository;
        }

        public List<Brand> GetAdminPageList(string search, int lang)
        {
            return BrandRepository.GetAdminPageList(search, lang);
        }

        public bool DeleteBrandById(int brandId)
        {
            return BrandRepository.DeleteByWhereCondition(r => r.Id == brandId);
        }

        public Brand GetBrandById(int brandId)
        {
            return BrandRepository.GetSingle(brandId);
        }
    }
}
