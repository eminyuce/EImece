using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IBrandRepository : IBaseContentRepository<Brand>
    {
        List<Brand> GetAdminPageList(string search, int lang);
    }
}