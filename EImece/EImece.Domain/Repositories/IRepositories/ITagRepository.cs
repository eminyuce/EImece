using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ITagRepository : IBaseEntityRepository<Tag>, IDisposable
    {
        List<Tag> GetAdminPageList(String search);
        Tag GetTagById(int tagId);
    }
}
