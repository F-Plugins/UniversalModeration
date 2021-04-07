using Microsoft.Extensions.Localization;
using OpenMod.API.Eventing;
using OpenMod.API.Users;
using OpenMod.Core.Users.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UniversalModeration.DataBase;

namespace UniversalModeration.Events
{
    public class UserConnectedEventListener : IEventListener<IUserConnectedEvent>
    {
        private readonly IMySqlDatabase m_Database;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserManager m_UserManager;
        public UserConnectedEventListener(IUserManager userManager, IStringLocalizer stringLocalizer,IMySqlDatabase database)
        {
            m_UserManager = userManager;
            m_StringLocalizer = stringLocalizer;
            m_Database = database;
        }

        public async Task HandleEventAsync(object? sender, IUserConnectedEvent @event)
        {
            var find = await m_Database.GetBanAsync(@event.User.Id);

            if (find != null)
            {
                if (find.expireDateTime > DateTime.Now)
                {
                    await m_UserManager.KickAsync(@event.User, m_StringLocalizer["plugin_translations:reason", new { Reason = find.banReason, Time = DateTime.Now.Second + find.expireDateTime.Second }]);
                    return;
                }
            }
        }
    }
}
