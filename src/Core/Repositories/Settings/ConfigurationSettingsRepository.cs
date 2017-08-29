using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories.Settings
{
    public class ConfigurationSettingsRepository : ISettingsRepository
    {
        IConfigurationRoot configurationRoot = null;
        public ConfigurationSettingsRepository(IConfigurationRoot _configurationRoot)
        {
            configurationRoot = _configurationRoot;
        }
        public async Task<T> Get<T>(string key)
        {
            return (T)(object)(configurationRoot.GetSection(key).Value);
        }
        public async Task Set<T>(string key, T value)
        {
            throw new NotImplementedException();
        }
    }
}
