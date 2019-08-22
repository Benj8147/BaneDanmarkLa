using BaneDanmarkLa.Pn.Infrastructure.Data.Seed.Data;
using Microting.BaneDanmarkLaBase.Infrastructure.Data;
using Microting.eFormApi.BasePn.Infrastructure.Database.Entities;
using System;
using Microting.eForm.Infrastructure.Constants;

namespace BaneDanmarkLa.Pn.Infrastructure.Data.Seed
{
    public class BaneDanmarkLaPluginSeed
    {
        public static void SeedData(BaneDanmarkLaPnDbContext dbContext)
        {
            var seedData = new BaneDanmarkLaConfigurationSeedData();
            var configurationList = seedData.Data;
            foreach (var configurationItem in configurationList)
            {
                if (!dbContext.PluginConfigurationValues.Any(x=>x.Name == configurationItem.Name))
                {
                    var newConfigValue = new PluginConfigurationValue()
                    {
                        Name = configurationItem.Name,
                        Value = configurationItem.Value,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Version = 1,
                        WorkflowState = Constants.WorkflowStates.Created,
                        CreatedByUserId = 1
                    };
                    dbContext.PluginConfigurationValues.Add(newConfigValue);
                    dbContext.SaveChanges();
                }
            }
        }
    }
}