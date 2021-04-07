using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.Core.Plugins;
using OpenMod.API.Plugins;
using UniversalModeration.DataBase;

[assembly: PluginMetadata("Feli.UniversalModeration", DisplayName = "UniversalModeration", Author = "Feli", Website = "")]
namespace UniversalModeration
{
    public class UniversalModeration : OpenModUniversalPlugin
    {
        private readonly ILogger<UniversalModeration> m_Logger;
        private readonly IMySqlDatabase m_MySqlDatabase;

        public UniversalModeration(
            IMySqlDatabase mySqlDatabase,
            ILogger<UniversalModeration> logger, 
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Logger = logger;
            m_MySqlDatabase = mySqlDatabase;
        }

        protected override async Task OnLoadAsync()
        {
            await m_MySqlDatabase.Reload();
            m_Logger.LogInformation("UniversalModeration 1.0.3 has been loaded");
        }

        protected override Task OnUnloadAsync()
        {
            m_Logger.LogInformation("UniversalModeration 1.0.3 has been unloaded");
            return Task.CompletedTask;
        }
    }
}
