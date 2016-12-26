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
            Expression<Func<Tag, object>> includeProperty2 = r => r.TagCategory;
            Expression<Func<Tag, object>>[] includeProperties = { includeProperty2 };
            var tags = TagRepository.GetAllIncluding(includeProperties);
            if (!String.IsNullOrEmpty(search))
            {
                tags = tags.Where(r => r.Name.ToLower().Contains(search.Trim().ToLower()));
            }
            var result = tags.OrderBy(r => r.Position).ThenByDescending(r => r.Id).ToList();

            return result;
        }
    }
}
