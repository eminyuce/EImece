using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface ITagService : IBaseEntityService<Tag>
    {
        List<Tag> GetAdminPageList(String search);
        void DeleteTagById(int tagId);
        Tag GetTagById(int tagId);
    }
}
