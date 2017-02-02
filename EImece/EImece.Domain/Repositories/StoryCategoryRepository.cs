using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;

namespace EImece.Domain.Repositories
{
    public class StoryCategoryRepository : BaseContentRepository<StoryCategory>, IStoryCategoryRepository
    {
        public StoryCategoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public StoryCategory GetStoryCategoryById(int storyCategoryId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MainImage);
            includeProperties.Add(r => r.Stories.Select(t => t.StoryFiles.Select(q => q.FileStorage)));
            includeProperties.Add(r => r.Stories.Select(t => t.StoryTags.Select(q => q.Tag)));
            var item = GetSingleIncluding(storyCategoryId, includeProperties.ToArray());
            return item;

        }
    }
}
