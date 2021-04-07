using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UniversalModeration.DataBase;

namespace UniversalModeration.Commands
{
    [Command("bans", Priority = OpenMod.API.Prioritization.Priority.Highest)]
    [CommandDescription("A command to check the bans of a user")]
    [CommandSyntax("/bans <userId>")]
    public class BansCommand : Command
    {
        private readonly IMySqlDatabase m_MySqlDatabase;
        private readonly IStringLocalizer m_StringLocalizer;
        public BansCommand(IStringLocalizer stringLocalizer, IMySqlDatabase mySqlDatabase ,IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_MySqlDatabase = mySqlDatabase;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Length < 1) throw new CommandWrongUsageException(Context);
            var userId = await Context.Parameters.GetAsync<string>(0);

            var search = await m_MySqlDatabase.GetBansAsync(userId);

            if (search == null)
            {
                throw new UserFriendlyException(m_StringLocalizer["plugin_translations:no_bans", new { Name = userId }]);
            }

            foreach(var ban in search)
            {
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["plugin_translations:bans", new { Punisher = ban.punisherId, Reason = ban.banReason, Time = ban.expireDateTime, Day = ban.banDateTime }], System.Drawing.Color.Magenta);
            }
        }
    }
}
