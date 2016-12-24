using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
   
    public abstract class BaseContentService<T> : BaseEntityService<T> where T : BaseContent
    {
        [Inject]
        public IProductRepository ProductRepository { get; set; }
        [Inject]
        public IStoryCategoryRepository StoryCategoryRepository { get; set; }
        [Inject]
        public IStoryRepository StoryRepository { get; set; }
        [Inject]
        public IProductCategoryRepository ProductCategoryRepository { get; set; }
        [Inject]
        public IMenuRepository MenuRepository { get; set; }
    }
}
