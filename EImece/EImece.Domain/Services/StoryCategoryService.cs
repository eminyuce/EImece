using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class StoryCategoryService : BaseContentService<StoryCategory>, IStoryCategoryService
    {
        private IStoryCategoryRepository StoryCategoryRepository { get; set; }
        public StoryCategoryService(IStoryCategoryRepository repository) : base(repository)
        {
            StoryCategoryRepository = repository;
        }

    }
}
