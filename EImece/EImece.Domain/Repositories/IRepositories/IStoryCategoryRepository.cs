using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IStoryCategoryRepository : IBaseContentRepository<StoryCategory>, IDisposable
    {
        StoryCategory GetStoryCategoryById(int storyCategoryId);
        List<StoryCategory> GetActiveStoryCategories(int language);
    }
}
