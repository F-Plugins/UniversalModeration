using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UniversalModeration.DataBase;
using UniversalModeration.WebHook;

namespace UniversalModeration.Commands
{
    [Command("ban", Priority = OpenMod.API.Prioritization.Priority.Highest)]
    [CommandDescription("A command to ban users using the UniversalModeration plugin")]
    [CommandSyntax("/ban <userName> Optional: (<reason> <time>)")]
    public class BanCommand : Command
    {
        private readonly IUserManager m_UserManager;
        private readonly IMySqlDatabase m_MySqlDatabase;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IWebhookService m_WebhookService;
        private readonly IConfiguration m_Configuration;
        public BanCommand(IConfiguration configuration, IWebhookService webhookService, IUserManager userManager, IMySqlDatabase mySqlDatabase, IStringLocalizer stringLocalizer, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_WebhookService = webhookService;
            m_UserManager = userManager;
            m_StringLocalizer = stringLocalizer;
            m_MySqlDatabase = mySqlDatabase;
        }

        protected override async Task OnExecuteAsync()
        {
            if (Context.Parameters.Length < 1) throw new CommandWrongUsageException(Context);

            var toBan = await Context.Parameters.GetAsync<IUser>(0);

            if (toBan == null)
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

            DateTime expireDate;
            if (Context.Parameters.Length >= 3)
            {
                var time = await Context.Parameters.GetAsync<double>(2);
                expireDate = DateTime.Now.AddSeconds(time);
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["plugin_translations:ban_success", new { Name = toBan.DisplayName, Reason = reason, Time = time }]);
            }
            else
            {
                expireDate = DateTime.MaxValue;
                await Context.Actor.PrintMessageAsync(m_StringLocalizer["plugin_translations:ban_success", new { Name = toBan.DisplayName, Reason = reason, Time = "permanent" }]);
            }

            await m_WebhookService.SendEmbedAsync(new Models.DiscordMessage
            {
                embeds = new List<Models.Embed>()
                {
                    new Models.Embed
                    {
                        title = "Ban Register",
                        color = 23131,
                        fields = new List<Models.Field>
                        {
                            new Models.Field
                            {
                                name = "UserName",
                                value = toBan.DisplayName,
                                inline = true
                            },
                            new Models.Field
                            {
                                name = "UserId",
                                value = toBan.Id,
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
                                name = "BanReason",
                                value = reason,
                                inline = true
                            },
                            new Models.Field
                            {
                                name = "BanTime",
                                value = expireDate.ToString(),
                                inline = true
                            }
                        },
                    }
                }
            }, m_Configuration.GetSection("plugin_configuration:WebHookURL").Get<string>());

            await m_MySqlDatabase.AddBanAsync(new Models.Ban
            {
                userId = toBan.Id,
                punisherId = Context.Actor.Id,
                banReason = reason,
                expireDateTime = expireDate,
                banDateTime = DateTime.Now
            });

            await m_UserManager.KickAsync(toBan, m_StringLocalizer["plugin_translations:reason", new { Reason = reason, Time = DateTime.Now.Second - expireDate.Second }]);
        }
    }
}
