using System;
using System.Collections.Generic;
using System.Reflection;
using BaneDanmarkLa.Pn.Abstractions;
using BaneDanmarkLa.Pn.Infrastructure.Data.Seed;
using BaneDanmarkLa.Pn.Infrastructure.Data.Seed.Data;
using BaneDanmarkLa.Pn.Infrastructure.Models.Settings;
using BaneDanmarkLa.Pn.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microting.eFormApi.BasePn;
using Microting.eFormApi.BasePn.Infrastructure.Database.Extensions;
using Microting.eFormApi.BasePn.Infrastructure.Models.Application;

namespace BaneDanmarkLa.Pn
{
    public class EformBaneDanmarkLaPlugin : IEformPlugin
    {
        public string Name => "Microting Bane Danmark LA Plugin";
        public string PluginId => "eform-angular-banedanmarkla-plugin";
        public string PluginPath => PluginAssembly().Location;
        private string _connectionString;

        public Assembly PluginAssembly()
        {
            return typeof(EformBaneDanmarkLaPlugin).GetTypeInfo().Assembly;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBaneDanmarkLaLocalizationService, BaneDanmarkLaLocalizationService>();
            services.AddTransient<IBaneDanmarkLaPnSettingsService, BaneDanmarkLaPnSettingsService>();
            services.AddTransient<IBaneDanmarkLaListCaseService, BaneDanmarkLaListCaseService>();
            services.AddTransient<IBaneDanmarkLaReportService, BaneDanmarkLaReportService>();
            services.AddTransient<IExcelService, ExcelService>();
            services.AddTransient<IBaneDanmarkLaListService, BaneDanmarkLaListService>();
            services.AddSingleton<IRebusService, RebusService>();
            
        }

        public void AddPluginConfig(IConfigurationBuilder builder, string connectionString)
        {
            var seedData = new BaneDanmarkLaConfigurationSeedData();
            var contextFactory = new BaneDanmarkLaPnContextFactory();
            builder.AddPluginConfiguration(
                connectionString,
                seedData,
                contextFactory);
        }

        public void ConfigureOptionsServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.ConfigurePluginDbOptions<BaneDanmarkLaBaseSettings>(
                configuration.GetSection("BaneDanmarkLaBaseSettings"));
        }

        public void ConfigureDbContext(IServiceCollection services, string connectionString)
        {
            _connectionString = connectionString;
            if (connectionString.ToLower().Contains("convert zero datetime"))
            {
                services.AddDbContext<BaneDanmarkLaPnDbContext>(o => o.UseMySql(connectionString,
                    b => b.MigrationsAssembly(PluginAssembly().FullName)));
            }
            else
            {
                services.AddDbContext<BaneDanmarkLaPnDbContext>(o => o.UseSqlServer(connectionString,
                    b => b.MigrationsAssembly(PluginAssembly().FullName)));
            }

            BaneDanmarkLaPnContextFactory contextFactory = new BaneDanmarkLaPnContextFactory();
            var context = contextFactory.CreateDbContext(new[] {connectionString});
            context.Database.Migrate();

            //Seed Database
            SeedDatabase(connectionString);
        }

        public void Configure(IApplicationBuilder appBuilder)
        {
            var serviceProvider = appBuilder.ApplicationServices;
            IRebusService rebusService = serviceProvider.GetService<IRebusService>();
            rebusService.Start(_connectionString);
        }


        public MenuModel HeaderMenu(IServiceProvider serviceProvider)
        {
            var localizationService = serviceProvider
                .GetService<IBaneDanmarkLaLocalizationService>();

            var result = new MenuModel();
            result.LeftMenu.Add(new MenuItemModel()
            {
                Name = localizationService.GetString("BaneDanmarkLa"),
                E2EId = "bane-danmark-la-pn",
                Link = "",
                MenuItems = new List<MenuItemModel>()
                {
                    new MenuItemModel()
                    {
                        Name = localizationService.GetString("Lists"),
                        E2EId = "bane-danmark-la-pn-lists",
                        Link = "/plugins/bane-danmark-la-pn/lists",
                        Position = 0,
                    },
                    new MenuItemModel()
                    {
                        Name = localizationService.GetString("Settings"),
                        E2EId = "bane-danmark-pn-settings",
                        Link = "/plugins/bane-danmark-la-pn/settings",
                        Position = 3,
                    }
                }
            });
            return result;
        }

        public void SeedDatabase(string connectionString)
        {
            // Get DbContext
            var contextFactory = new BaneDanmarkLaPnContextFactory();
            using (var context = contextFactory.CreateDbContext(new[] {connectionString}))
            {
                // Seed configuration
                BaneDanmarkLaPluginSeed.SeedData(context);
            }
        }
    }
}