using HarmonyLib;
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
    [Command("warns", Priority = OpenMod.API.Prioritization.Priority.Highest)]
    [CommandDescription("A command to check the warns of a user")]
    [CommandSyntax("/warns <userId>")]
    public class WarnsCommand : Command
    {
        private readonly IMySqlDatabase m_MySqlDatabase;
        private readonly IStringLocalizer m_StringLocalizer;

        public WarnsCommand(IStringLocalizer stringLocalizer, IMySqlDatabase mySqlDatabase, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_MySqlDatabase = mySqlDatabase;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Length < 1) throw new CommandWrongUsageException(Context);
            var userId = await Context.Parameters.GetAsync<string>(0);

            var search = await m_MySqlDatabase.GetWarnsAsync(userId);

            if (search == null)
            {
                throw new UserFriendlyException(m_StringLocalizer["plugin_translations:no_warns", new { Name = userId }]);
            }

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["plugin_translations:warns_count", new { Count = search.Count }], System.Drawing.Color.Magenta);

            foreach (var warn in search)
            {
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["plugin_translations:warns", new { Punisher = warn.punisherId, Reason = warn.warnReason, Day = warn.warnDateTime }], System.Drawing.Color.Magenta);
            }
        }
    }
}
