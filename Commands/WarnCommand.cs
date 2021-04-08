using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UniversalModeration.DataBase;
using UniversalModeration.WebHook;

namespace UniversalModeration.Commands
{
    [Command("warn", Priority = OpenMod.API.Prioritization.Priority.Highest)]
    [CommandDescription("A command to warn a user")]
    [CommandSyntax("/warn <userName> Optional: (<reason>)")]
    public class WarnCommand : Command
    {
        private readonly IMySqlDatabase m_MySqlDatabase;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IWebhookService m_WebhookService;
        private readonly IConfiguration m_Configuration;

        public WarnCommand(IConfiguration configuration, IWebhookService webhookService, IMySqlDatabase mySqlDatabase, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_WebhookService = webhookService;
            m_StringLocalizer = stringLocalizer;
            m_MySqlDatabase = mySqlDatabase;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Length < 1) throw new CommandWrongUsageException(Context);

            var toWarn = await Context.Parameters.GetAsync<IUser>(0);

            if (toWarn == null)
            {
                throw new UserFriendlyException(m_StringLocalizer["plugin_translations:not_found", new { Name = await Context.Parameters.GetAsync<string>(0) }]);
            }

            string reason;

            if (Context.Parameters.Length >= 2)
            {
                reason = await Context.Parameters.GetAsync<string>(1);
            }
            else
            {
                reason = m_StringLocalizer["plugin_translations:no_reason"];
            }

            await m_WebhookService.SendEmbedAsync(new Models.DiscordMessage
            {
                embeds = new List<Models.Embed>()
                {
                    new Models.Embed
                    {
                        title = "Warn Register",
                        color = 23131,
                        fields = new List<Models.Field>
                        {
                            new Models.Field
                            {
                                name = "UserName",
                                value = toWarn.DisplayName,
                                inline = true
                            },
                            new Models.Field
                            {
                                name = "UserId",
                                value = toWarn.Id,
                                inline = true
                            },
                            new Models.Field
                            {
                                name = "PunisherName",
                                value = Context.Actor.DisplayName,
                                inline = true
                            },
                            new Models.Field
                            {
                                name = "PunisherId",
                                value = Context.Actor.Id,
                                inline = true
                            },
                            new Models.Field
                            {
                                name = "WarnReason",
                                value = reason,
                                inline = true
                            },
                        },
                    }
                }
            }, m_Configuration.GetSection("plugin_configuration:WarnWebHookURL").Get<string>());

            await m_MySqlDatabase.AddWarnAsync(new Models.Warn
            {
                userId = toWarn.Id,
                punisherId = Context.Actor.Id,
                warnReason = reason,
                warnDateTime = DateTime.Now
            });

            await Context.Actor.PrintMessageAsync(m_StringLocalizer["plugin_translations:warn_success", new { Name = toWarn.DisplayName, Reason = reason }], System.Drawing.Color.Magenta);
        }
    }
}
