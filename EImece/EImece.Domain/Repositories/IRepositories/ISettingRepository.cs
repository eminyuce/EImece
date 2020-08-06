using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ISettingRepository : IBaseEntityRepository<Setting>
    {
        List<Setting> GetAllSettings();

        List<Setting> GetAllActiveSettings();
    }
}