using OpenMod.API.Ioc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UniversalModeration.Models;

namespace UniversalModeration.WebHook
{
    [Service]
    public interface IWebhookService
    {
        Task SendEmbedAsync(DiscordMessage message, string url);
    }
}
