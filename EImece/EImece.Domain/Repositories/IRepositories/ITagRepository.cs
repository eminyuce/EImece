using EImece.Domain.Entities;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ITagRepository : IBaseEntityRepository<Tag>
    {
        List<Tag> GetAdminPageList(String search, int language);

        Tag GetTagById(int tagId);
    }
}