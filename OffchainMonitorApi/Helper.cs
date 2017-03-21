using Core.Repositories.Settings;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OffchainMonitorApi
{
    public sealed class Helper
    {
        public static async Task<Network> GetNetwork(ISettingsRepository settingsRepository)
        {
            string network = await settingsRepository.Get<string>("network");
            switch (network)
            {
                case "main":
                    return Network.Main;
                default:
                    return Network.TestNet;
            }
        }
    }
}
