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
    public class StoryService : BaseContentService<Story>, IStoryService
    {
        private IStoryRepository StoryRepository { get; set; }
        public StoryService(IStoryRepository repository):base(repository)
        {
            StoryRepository = repository;
            
        }



    }
}
