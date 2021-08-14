using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Linq;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using OpenMod.Core.Users.Events;
using OpenMod.Extensions.Games.Abstractions.Players;
using System.Diagnostics.Tracing;
using System.Net.Http;
using System.Threading.Tasks;
using UniversalModeration.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System;
using UniversalModeration.Enums;
using OpenMod.API.Users;
using BanData = UniversalModeration.Models.BanData;
using UniversalModeration.Helpers;
using UserData = UniversalModeration.Models.UserData;

namespace UniversalModeration.Events
{
    [EventListenerLifetime(ServiceLifetime.Transient)]
    public class UserConnectingEventListner : IEventListener<IUserConnectingEvent>
    {
        private readonly UniversalModerationDbContext database;
        private readonly Logger<UserConnectingEventListner> logger;
        private readonly IConfiguration configuration;
        private readonly IStringLocalizer stringLocalizer;

        public UserConnectingEventListner(IConfiguration configuration, 
            IStringLocalizer stringLocalizer, 
            Logger<UserConnectingEventListner> logger, 
            UniversalModerationDbContext database)
        {
            this.database = database;
            this.configuration = configuration;
            this.stringLocalizer = stringLocalizer;
            this.logger = logger;
        }

        [EventListener(IgnoreCancelled = true, Priority = EventListenerPriority.Highest)]
        public async Task HandleEventAsync(object sender, IUserConnectingEvent @event)
        {
            var user = await database.Users.Include(x => x.Bans).FirstOrDefaultAsync(x => x.UserId == @event.User.Id);

            var playerUser = @event.User as IPlayerUser;

            var ip = playerUser.Player.Address.MapToIPv4().ToString();

            if(user == null)
            {
                var response = await new HttpClient().GetAsync(string.Format("http://ip-api.com/json/{0}", ip));

                if (response.IsSuccessStatusCode)
                {
                    JObject @object = JObject.Parse(await response.Content.ReadAsStringAsync());

                    if((string)@object["status"] == "success")
                    {
                        await database.Users.AddAsync(new UserData
                        {
                            UserId = playerUser.Id,
                            UserName = playerUser.DisplayName,
                            Ip = ip,
                            City = (string)@object["city"],
                            Country = (string)@object["country"]
                        });

                        user = await database.Users.Include(x => x.Bans).FirstOrDefaultAsync(x => x.UserId == @event.User.Id);
                    }
                    else
                    {
                        logger.LogWarning("An unexpected error ocurred while performing a web request to ip-api.com");
                        logger.LogWarning("Error message: {Message}", new { Message = (string)@object["message"] });
                        return;
                    }
                }
                else
                {
                    logger.LogWarning("An unexpected error ocurred while performing a web request to ip-api.com");
                    return;
                }
            }

            if(user.Bans.Any(x => x.IsBanned))
            {
                await @event.RejectAsync(stringLocalizer["rejectionMessages:ban", new
                {
                    BanData = await database.Bans.Include(x => x.User).Include(x => x.PunisherId).LastOrDefaultAsync(x => x.UserId == user.UserId)
                }]);
                return;
            }

            if (configuration.GetSection("bansConfiguration:autoBans:vpn:enabled").Get<bool>() && await VpnDetectionHelper.IsVpn(ip, configuration["bansConfiguration:autoBans:vpn:apiKey"]))
            {
                var vpnType = (EBanType)Enum.Parse(typeof(EBanType), configuration["bansConfiguration:autoBans:vpn:timeMode"]);

                switch (banType)
                {
                    case EBanType.Kick:
                        await @event.RejectAsync(stringLocalizer["rejectionMessages:vpn:kick"]);
                        break;
                    case EBanType.Permanent:
                        await database.Bans.AddAsync(new BanData
                        {
                            UserId = playerUser.Id,
                            BanDate = DateTime.Now,
                            PunisherId = playerUser.Id,
                            UnbanDate = DateTime.MaxValue,
                            Reason = stringLocalizer["rejectionMessages:vpn:banReason"]
                        });

                        await @event.RejectAsync(stringLocalizer["rejectionMessages:ban", new
                        {
                            BanData = await database.Bans.Include(x => x.User).Include(x => x.Punisher).LastOrDefaultAsync(x => x.UserId == playerUser.Id)
                        }]);
                        break;
                    case EBanType.Time:
                        await database.Bans.AddAsync(new BanData
                        {
                            UserId = playerUser.Id,
                            BanDate = DateTime.Now,
                            PunisherId = playerUser.Id,
                            UnbanDate = DateTime.Now.AddSeconds(configuration.GetSection("bansConfiguration:autoBans:vpn:time").Get<double>()),
                            Reason = stringLocalizer["rejectionMessages:vpn:banReason"]
                        });

                        await @event.RejectAsync(stringLocalizer["rejectionMessages:ban", new
                        {
                            BanData = await database.Bans.Include(x => x.User).Include(x => x.Punisher).LastOrDefaultAsync(x => x.UserId == playerUser.Id)
                        }]);
                        break;
                }
            }

            var banEvading = await database.Bans.Include(x => x.User).Include(x => x.Punisher).LastOrDefaultAsync(x => x.IsBanned && x.User.Ip == ip);

            if (banEvading != null)
            {
                var banType = (EBanType)Enum.Parse(typeof(EBanType), configuration["bansConfiguration:autoBans:evading"]);

                switch (banType)
                {
                    case EBanType.Kick:
                        await @event.RejectAsync(stringLocalizer["rejectionMessages:kick", new
                        {
                            BanData = banEvading
                        }]);
                        break;
                    case EBanType.Permanent:
                        await database.Bans.AddAsync(new BanData
                        {
                            UserId = playerUser.Id,
                            BanDate = DateTime.Now,
                            PunisherId = banEvading.PunisherId,
                            Reason = banEvading.Reason,
                            UnbanDate = DateTime.MaxValue
                        });

                        await @event.RejectAsync(stringLocalizer["rejectionMessages:ban", new
                        {
                            BanData = await database.Bans.Include(x => x.User).Include(x => x.Punisher).LastOrDefaultAsync(x => x.UserId == playerUser.Id)
                        }]);
                        break;
                    case EBanType.Time:
                        await database.Bans.AddAsync(new BanData
                        {
                            UserId = playerUser.Id,
                            BanDate = DateTime.Now,
                            PunisherId = banEvading.PunisherId,
                            Reason = banEvading.Reason,
                            UnbanDate = DateTime.Now.AddSeconds(configuration.GetSection("bansConfiguration:autoBans:evading:time").Get<double>())
                        });

                        await @event.RejectAsync(stringLocalizer["rejectionMessages:ban", new
                        {
                            BanData = await database.Bans.Include(x => x.User).Include(x => x.Punisher).LastOrDefaultAsync(x => x.UserId == playerUser.Id)
                        }]);
                        break;
                }
            }
        }
    }
}
