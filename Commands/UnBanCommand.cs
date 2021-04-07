using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UniversalModeration.DataBase;
using UniversalModeration.WebHook;

namespace UniversalModeration.Commands
{
    [Command("unban", Priority = OpenMod.API.Prioritization.Priority.Highest)]
    [CommandDescription("A command to unban a user")]
    [CommandSyntax("/unban <userId>")]
    public class UnBanCommand : Command
    {
        private readonly IMySqlDatabase m_MySqlDatabase;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IWebhookService m_WebhookService;
        private readonly IConfiguration m_Configuration;

        public UnBanCommand(IMySqlDatabase database ,IWebhookService webhookService, IConfiguration configuration, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_MySqlDatabase = database;
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
            m_WebhookService = webhookService;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Length < 1) throw new CommandWrongUsageException(Context);

            var userId = await Context.Parameters.GetAsync<string>(0);

            var find = await m_MySqlDatabase.GetBanAsync(userId);

            if (find != null)
            {
                throw new UserFriendlyException(m_StringLocalizer["plugin_translations:uban_not_found", new { Name = userId }]);
            }

            await m_MySqlDatabase.UpdateLastBanAsync(userId, true);

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["plugin_translations:unban_success", new { Name = userId }]);

            await m_WebhookService.SendEmbedAsync(new Models.DiscordMessage
            {
                embeds = new List<Models.Embed>
                {
                    new Models.Embed
                    {
                        title = "UnBan Register",
                        color = 23131,
                        fields = new List<Models.Field>
                        {
                            new Models.Field
                            {
                                name = "UserId",
                                value = userId,
                                inline = true
                            },
                            new Models.Field
                            {
                                name = "ModeratorName",
                                value = Context.Actor.DisplayName,
                                inline = true
                            },
                            new Models.Field
                            {
                                name = "ModeratorId",
                                value = Context.Actor.Id,
                                inline = true
                            }
                        },
                    }
                }
            }, m_Configuration.GetSection("plugin_configuration:UnBanWebHookURL").Get<string>());
        }
    }
}
