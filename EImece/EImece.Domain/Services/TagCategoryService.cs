using EImece.Domain.Entities;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Models.Enums;
using Ninject;

namespace EImece.Domain.Services
{
    public class TagCategoryService : BaseEntityService<TagCategory>, ITagCategoryService
    {
        [Inject]
        public ITagService TagService { get; set; }

        private ITagCategoryRepository TagCategoryRepository { get; set; }
        public TagCategoryService(ITagCategoryRepository repository) : base(repository)
        {
            TagCategoryRepository = repository;
        }
        
        public List<TagCategory> GetTagsByTagType(EImeceLanguage language)
        {
            return TagCategoryRepository.GetTagsByTagType(language);
        }

        public void DeleteTagCategoryById(int tagCategoryId)
        {
            var tagCategory = GetTagCategoryById(tagCategoryId);
            var tagIdList = tagCategory.Tags.Select(r => r.Id).ToList();
            foreach (var tagId in tagIdList)
            {
                TagService.DeleteTagById(tagId);
            }
            DeleteEntity(tagCategory);
        }

        public TagCategory GetTagCategoryById(int tagCategoryId)
        {
           return TagCategoryRepository.GetTagCategoryById(tagCategoryId);
        }
    }
}
