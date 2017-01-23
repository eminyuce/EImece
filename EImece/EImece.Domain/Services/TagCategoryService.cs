using EImece.Domain.Entities;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Models.Enums;

namespace EImece.Domain.Services
{
    public class TagCategoryService : BaseEntityService<TagCategory>, ITagCategoryService
    {
        private ITagCategoryRepository TagCategoryRepository { get; set; }
        public TagCategoryService(ITagCategoryRepository repository) : base(repository)
        {
            TagCategoryRepository = repository;
        }
        
        public List<TagCategory> GetTagsByTagType(EImeceLanguage language)
        {
            return TagCategoryRepository.GetTagsByTagType(language);
        }
    }
}
