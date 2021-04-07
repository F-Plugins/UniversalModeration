using OpenMod.API.Ioc;
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
        Task<Ban> GetBanAsync(string userId);
        Task AddBanAsync(Ban ban);
        Task Reload();
    }
}
