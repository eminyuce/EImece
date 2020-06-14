using EImece.Domain.Entities;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface ISettingRepository : IBaseEntityRepository<Setting>, IDisposable
    {
        List<Setting> GetAllSettings();

        List<Setting> GetAllActiveSettings();
    }
}