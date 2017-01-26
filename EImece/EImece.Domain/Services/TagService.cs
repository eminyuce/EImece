using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class TagService : BaseEntityService<Tag>, ITagService
    {
        private ITagRepository TagRepository { get; set; }
        public TagService(ITagRepository repository) : base(repository)
        {
            TagRepository = repository;
        }

        public List<Tag> GetAdminPageList(String search)
        {
            return TagRepository.GetAdminPageList(search);
        }

        public void DeleteTagById(int tagId)
        {
            var tag = GetTagById(tagId);
            ProductTagRepository.DeleteByWhereCondition(r => r.TagId == tagId);
            StoryTagRepository.DeleteByWhereCondition(r => r.TagId == tagId);
            DeleteEntity(tag);
        }

        public Tag GetTagById(int tagId)
        {
            return TagRepository.GetTagById(tagId);
        }
    }
}
