﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Settings
{
    public class SettingsProvider : ISettingsProvider
    {
        static Settings settings = new Settings();

        static SettingsProvider()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            settings.AzureStorageEmulatorPath = config.AppSettings.Settings["AzureStorageEmulatorPath"]?.Value;
            settings.BitcoinDaemonPath = config.AppSettings.Settings["BitcoinDaemonPath"]?.Value;
            settings.BitcoinWorkingPath = config.AppSettings.Settings["BitcoinWorkingPath"]?.Value;
            settings.RegtestRPCUsername = config.AppSettings.Settings["RegtestRPCUsername"]?.Value;
            settings.RegtestRPCPassword = config.AppSettings.Settings["RegtestRPCPassword"].Value;
            settings.RegtestRPCIP = config.AppSettings.Settings["RegtestRPCIP"]?.Value;
            settings.RegtestPort = int.Parse(config.AppSettings.Settings["RegtestPort"]?.Value);
            settings.QBitNinjaListenerConsolePath = config.AppSettings.Settings["QBitNinjaListenerConsolePath"]?.Value;
            settings.WalletBackendExecutablePath = config.AppSettings.Settings["WalletBackendExecutablePath"]?.Value;
            settings.InQueueConnectionString = config.AppSettings.Settings["InQueueConnectionString"]?.Value;
            settings.OutQueueConnectionString = config.AppSettings.Settings["OutQueueConnectionString"]?.Value;
            settings.DBConnectionString = config.AppSettings.Settings["DBConnectionString"]?.Value;
            settings.ExchangePrivateKey = config.AppSettings.Settings["ExchangePrivateKey"]?.Value;
            settings.Network = config.AppSettings.Settings["Network"].Value.ToLower().Equals("main") ? NBitcoin.Network.Main : NBitcoin.Network.TestNet;
            settings.QBitNinjaBaseUrl = config.AppSettings.Settings["QBitNinjaBaseUrl"]?.Value;
            settings.WalletBackendUrl = config.AppSettings.Settings["WalletBackendUrl"]?.Value;
        }

        public Settings GetSettings()
        {
            return settings;
        }
    }
}
