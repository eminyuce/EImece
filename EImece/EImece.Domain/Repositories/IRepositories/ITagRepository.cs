using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ITagRepository : IBaseEntityRepository<Tag>
    {
        List<Tag> GetAdminPageList(String search, int language);

        Task<List<Tag>> GetAdminPageListAsync(String search, int language);

        Tag GetTagById(int tagId);
        List<Tag> GetProductTags(int language);
    }
}