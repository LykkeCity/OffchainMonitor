using Core.Repositories.Settings;
using Microsoft.EntityFrameworkCore;
using SqlliteRepositories.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SqlliteRepositories.Settings
{
    public class SettingsRepository : ISettingsRepository
    {
        // ToDo: Use the DbContext from the caller
        public async Task<T> Get<T>(string key)
        {
            using (OffchainMonitorContext context = new OffchainMonitorContext())
            {
                var settingRecords = await (from record in context.Settings where record.Key == key select record)
                    .FirstOrDefaultAsync();
                if (settingRecords == null)
                {
                    throw new Exception("The specified settings was not found.");
                }
                else
                {
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)settingRecords.Value;
                    }
                    else
                    {
                        return default(T);
                    }
                }
            }
        }

        public async Task Set<T>(string key, T value)
        {
            using (OffchainMonitorContext context = new OffchainMonitorContext())
            {
                var settingRecords = from record in context.Settings where record.Key == key select record;
                if (settingRecords.Count() > 0)
                {
                    context.Settings.RemoveRange(settingRecords);
                }

                context.Settings.Add(new SettingsEntity { Key = key.ToString(), Value = value.ToString() });

                await context.SaveChangesAsync();
            }
        }
    }
}
