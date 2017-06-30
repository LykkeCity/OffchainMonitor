using Common.Assets;
using Common.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Settings
{
    public interface IBlockchainStateManagerSettings : ISettings
    {
        string AzureStorageEmulatorPath { get; set; }
        string BitcoinDaemonPath { get; set; }

        string BitcoinWorkingPath { get; set; }
        int ColorCorePort { get; set; }
        string QBitNinjaListenerConsolePath { get; set; }
        string RegtestRPCUsername { get; set; }

        string RegtestRPCPassword { get; set; }

        string RegtestRPCIP { get; set; }

        int RegtestPort { get; set; }
        AssetDefinition[] Assets { get; set; }
        string FeeAddress { get; set; }
    }
}
