using Newtonsoft.Json;
using OpenMod.API.Ioc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UniversalModeration.Models;

namespace UniversalModeration.WebHook
{
    [PluginServiceImplementation(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class WebhookService : IWebhookService
    {
        public Task SendEmbedAsync(DiscordMessage message, string url)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                wc.UploadStringAsync(new Uri(url), JsonConvert.SerializeObject(message));
            }
            return Task.CompletedTask;
        }
    }
}
