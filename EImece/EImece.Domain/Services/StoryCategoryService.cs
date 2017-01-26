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


        [Inject]
        public IStoryService StoryService { get; set; }

        private IStoryCategoryRepository StoryCategoryRepository { get; set; }
        public StoryCategoryService(IStoryCategoryRepository repository) : base(repository)
        {
            StoryCategoryRepository = repository;
        }
        public StoryCategory GetStoryCategoryById(int storyCategoryId)
        {
            return StoryCategoryRepository.GetStoryCategoryById(storyCategoryId);
        }
        public void DeleteStoryCategoryById(int storyCategoryId)
        {
            var storyCategory = GetStoryCategoryById(storyCategoryId);
            if (storyCategory.MainImageId.HasValue)
            {
                FileStorageService.DeleteFileStorage(storyCategory.MainImageId.Value);
            }
            var storyIdList = storyCategory.Stories.Select(r => r.Id).ToList();
            foreach (var id in storyIdList)
            {
                StoryService.DeleteStoryById(id);
            }
            DeleteEntity(storyCategory);
        }
    }
}
