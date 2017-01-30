using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericRepository;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IStoryRepository : IBaseContentRepository<Story>, IDisposable
    {
        List<Story> GetAdminPageList(int categoryId, string search, int lang);
        Story GetStoryById(int storyId);
        PaginatedList<Story> GetMainPageStories(int page, int pageSize, int language);
        List<Story> GetRelatedStories(int[] tagIdList, int take, int lang);
    }
}
