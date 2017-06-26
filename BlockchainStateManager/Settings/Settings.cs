using BlockchainStateManager.Assets;
using BlockchainStateManager.Enum;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Settings
{
    public class Settings : TheSettings
    {
        public string AzureStorageEmulatorPath
        {
            get;
            set;
        }

        public string BitcoinDaemonPath
        {
            get;
            set;
        }

        public string BitcoinWorkingPath
        {
            get;
            set;
        }

        public string RegtestRPCUsername
        {
            get;
            set;
        }

        public string RegtestRPCPassword
        {
            get;
            set;
        }

        public string RegtestRPCIP
        {
            get;
            set;
        }

        public int RegtestPort
        {
            get;
            set;
        }

        public int ColorCorePort
        {
            get;
            set;
        }

        public string QBitNinjaListenerConsolePath
        {
            get;
            set;
        }

        public string DBConnectionString
        {
            get;
            set;
        }

        public Network Network
        {
            get;
            set;
        }

        public string ExchangePrivateKey
        {
            get;
            set;
        }

        internal const APIProvider ApiProvider = APIProvider.QBitNinja;
    }
}
