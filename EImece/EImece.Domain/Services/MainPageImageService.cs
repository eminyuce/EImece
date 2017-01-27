using EImece.Domain.Entities;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Repositories.IRepositories;
using System.Linq.Expressions;
using Ninject;
using EImece.Domain.Models.FrontModels;

namespace EImece.Domain.Services
{
    public class MainPageImageService : BaseContentService<MainPageImage>, IMainPageImageService
    {
        [Inject]
        public IProductService ProductService { get; set; }

        private IMainPageImageRepository MainPageImageRepository { get; set; }
        public MainPageImageService(IMainPageImageRepository repository) : base(repository)
        {
            MainPageImageRepository = repository;
        }

        public void DeleteMainPageImage(int id)
        {
            var item = MainPageImageRepository.GetSingle(id);
            if (item.MainImageId.HasValue)
            {
                FileStorageService.DeleteFileStorage(item.MainImageId.Value);
            }
            DeleteEntity(item);
        }
        public MainPageViewModel GetMainPageViewModel(int language)
        {
            var cacheKey = String.Format("GetMainPageViewModel-{0}", language);
            MainPageViewModel result = null;

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = new MainPageViewModel();
                result.MainPageProducts = ProductService.GetMainPageProducts(1,  language).Products;
                result.MainPageImages = MainPageImageRepository.GetActiveBaseContents(true, language);
                MemoryCacheProvider.Set(cacheKey, result, Settings.CacheMediumSeconds);
            }
            return result;
        }
    }
}
