using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UniversalModeration.WebHook;

namespace UniversalModeration.Commands
{
    [Command("kick", Priority = OpenMod.API.Prioritization.Priority.Highest)]
    [CommandDescription("A command to kick a user")]
    [CommandSyntax("/kick <userName> Optional: (<reason>)")]
    public class KickCommand : Command
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IWebhookService m_WebhookService;
        private readonly IConfiguration m_Configuration;
        private readonly IUserManager m_UserManager;
        public KickCommand(IUserManager userManager, IWebhookService webhookService, IConfiguration configuration, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_UserManager = userManager;
            m_StringLocalizer = stringLocalizer;
            m_Configuration = configuration;
            m_WebhookService = webhookService;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Length < 1) throw new CommandWrongUsageException(Context);

            var toKick = await Context.Parameters.GetAsync<IUser>(0);

            if (toKick == null)
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
                        title = "Kick Register",
                        color = 23131,
                        fields = new List<Models.Field>
                        {
                            new Models.Field
                            {
                                name = "UserName",
                                value = toKick.DisplayName,
                                inline = true
                            },
                            new Models.Field
                            {
                                name = "UserId",
                                value = toKick.Id,
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
                                name = "KickReason",
                                value = reason,
                                inline = true
                            },
                        },
                    }
                }
            }, m_Configuration.GetSection("plugin_configuration:KickWebHookURL").Get<string>());
            await Context.Actor.PrintMessageAsync(m_StringLocalizer["plugin_translations:kick_success", new { Name = toKick.DisplayName, Reason = reason }], System.Drawing.Color.Magenta);
            await m_UserManager.KickAsync(toKick, reason);
        }
    }
}
