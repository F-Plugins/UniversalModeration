using Microsoft.EntityFrameworkCore;
using UniversalModeration.Models;
using OpenMod.EntityFrameworkCore;
using OpenMod.EntityFrameworkCore.Configurator;
using System;
using System.Security.Cryptography.X509Certificates;

namespace UniversalModeration.Database
{
    public class UniversalModerationDbContext : OpenModDbContext<UniversalModerationDbContext>
    {
        public DbSet<UserData> Users => Set<UserData>();
        public DbSet<BanData> Bans => Set<BanData>();

        public UniversalModerationDbContext(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public UniversalModerationDbContext(IDbContextConfigurator configurator, IServiceProvider serviceProvider) : base(configurator, serviceProvider)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BanData>()
                .HasIndex(x => x.UserId);
        }
    }
}
