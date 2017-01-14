using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using System.Linq.Expressions;

namespace EImece.Domain.Repositories
{
    public class StoryRepository : BaseContentRepository<Story>, IStoryRepository
    {
        public StoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<Story> GetAdminPageList(int categoryId, string search, int lang)
        {
            Expression<Func<Story, object>> includeProperty3 = r => r.MainImage;
            Expression<Func<Story, object>> includeProperty2 = r => r.StoryCategory;
            Expression<Func<Story, object>>[] includeProperties = { includeProperty2, includeProperty3 };
            var stories = GetAllIncluding(includeProperties).Where(r => r.Lang == lang);
            if (!String.IsNullOrEmpty(search))
            {
                stories = stories.Where(r => r.Name.ToLower().Contains(search));
            }
            if (categoryId > 0)
            {
                stories = stories.Where(r => r.StoryCategoryId == categoryId);
            }
            stories = stories.OrderBy(r => r.Position).ThenByDescending(r => r.Id);

            return stories.ToList();
        }
    }
}
