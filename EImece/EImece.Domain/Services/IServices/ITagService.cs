using EImece.Domain.Entities;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface ITagService : IBaseEntityService<Tag>
    {
        List<Tag> GetAdminPageList(String search, int language);

        void DeleteTagById(int tagId);

        Tag GetTagById(int tagId);
    }
}