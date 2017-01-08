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
            var result = new MainPageViewModel();

            result.MainPageProducts = ProductService.GetMainPageProducts(1, 5, language);
            result.MainPageImages = MainPageImageRepository.GetActiveBaseContents(true, language);

            return result;
        }
    }
}
