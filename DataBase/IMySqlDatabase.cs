using OpenMod.API.Ioc;
using OpenMod.API.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UniversalModeration.Models;

namespace UniversalModeration.DataBase
{
    [Service]
    public interface IMySqlDatabase
    {
        Task<List<Ban>> GetBansAsync(string userId);
        Task<Ban> GetBanAsync(string userId);
        Task UpdateLastBanAsync(string userId, bool unBanned);
        Task AddBanAsync(Ban ban);

        Task<List<Warn>> GetWarnsAsync(string userId);
        Task AddWarnAsync(Warn warn);

        Task Reload();
    }
}
