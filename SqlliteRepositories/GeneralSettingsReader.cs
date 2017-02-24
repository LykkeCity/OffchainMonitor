using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SqlliteRepositories
{
    public static class GeneralSettingsReader
    {
        public static T ReadGeneralSettings<T>(IConfigurationRoot configuration, string configName)
        {
            var exists = configuration.GetChildren().Where(c => c.Key == configName).Any();

            string str = configuration.GetChildren().Where(c => c.Key == configName)
                .FirstOrDefault().Value;
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
        }
    }
}
