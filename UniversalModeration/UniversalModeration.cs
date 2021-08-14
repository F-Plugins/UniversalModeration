using System;
using OpenMod.Core.Plugins;
using OpenMod.API.Plugins;
using OpenMod.EntityFrameworkCore.MySql.Extensions;
using UniversalModeration.Database;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

[assembly: PluginMetadata("Feli.UniversalModeration", DisplayName = "UniversalModeration")]
namespace UniversalModeration
{
    public class UniversalModeration : OpenModUniversalPlugin
    {
        private readonly UniversalModerationDbContext database;
        public UniversalModeration(
            UniversalModerationDbContext database,
            IServiceProvider serviceProvider) : base(serviceProvider) 
        {
            this.database = database;
        }

        protected override async Task OnLoadAsync()
        {
            await database.Database.MigrateAsync();
        }
    }

    public class PluginContainerConfigurator : IPluginContainerConfigurator
    {
        public void ConfigureContainer(IPluginServiceConfigurationContext context)
        {
            context.ContainerBuilder.AddMySqlDbContext<UniversalModerationDbContext>();
        }
    }
}
