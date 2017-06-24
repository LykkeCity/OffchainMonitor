using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockchainStateManager.Assets;
using BlockchainStateManager.Enum;

namespace BlockchainStateManager.Settings
{
    public class TheSettings
    {
        public string RestEndPoint { get; set; }
        public string InQueueConnectionString { get; set; }
        public string OutQueueConnectionString { get; set; }

        public string ConnectionString { get; set; }
        public string LykkeSettingsConnectionString { get; set; }

        public AssetDefinition[] Assets { get; set; }
        public NetworkType NetworkType { get; set; }
        public string exchangePrivateKey { get; set; }
        public string RPCUsername { get; set; }
        public string RPCPassword { get; set; }
        public string RPCServerIpAddress { get; set; }
        public string FeeAddress { get; set; }
        public string FeeAddressPrivateKey { get; set; }

        public string QBitNinjaBaseUrl { get; set; }
        public string QBitNinjaBalanceUrl
        {
            get
            {
                return QBitNinjaBaseUrl + "balances/";
            }
        }

        public string QBitNinjaTransactionUrl
        {
            get
            {
                return QBitNinjaBaseUrl + "transactions/";
            }
        }

        public string WalletBackendUrl { get; set; }
        public int PreGeneratedOutputMinimumCount { get; set; }

        [DefaultValue("outdata")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string OutdataQueueName
        {
            get;
            set;
        }

        [DefaultValue(null)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string EnvironmentName
        {
            get;
            set;
        }

        [DefaultValue(1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public double FeeMultiplicationFactor
        {
            get;
            set;
        }
        
        [DefaultValue(400)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int BroadcastGroup
        {
            get;
            set;
        }

        [DefaultValue(0)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int SwapMinimumConfirmationNumber
        {
            get;
            set;
        }

        [DefaultValue(0)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int TransferFromPrivateWalletMinimumConfirmationNumber
        {
            get;
            set;
        }

        [DefaultValue(0)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int TransferFromMultisigWalletMinimumConfirmationNumber
        {
            get;
            set;
        }

        [DefaultValue(1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int GenerateRefundingTransactionMinimumConfirmationNumber
        {
            get;
            set;
        }

        [DefaultValue(1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int DefaultNumberOfRequiredConfirmations
        {
            get;
            set;
        }

        [DefaultValue(200000)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MaximumTransactionSendFeesInSatoshi
        {
            get;
            set;
        }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool PrivateKeyWillBeSubmitted
        {
            get;
            set;
        }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UseMockAsLykkeNotification
        {
            get;
            set;
        }

        [DefaultValue(10 * 60 * 1000)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int UnsignedTransactionsUpdaterPeriod
        {
            get;
            set;
        }

        [DefaultValue(5)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int UnsignedTransactionTimeoutInMinutes
        {
            get;
            set;
        }

        [DefaultValue(5000)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int QueueReaderIntervalInMiliseconds
        {
            get;
            set;
        }

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UseSegKeysTable
        {
            get;
            set;
        }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsConfigurationEncrypted
        {
            get;
            set;
        }

        [DefaultValue(60)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public uint FeeReserveCleanerTimerPeriodInSeconds
        {
            get;
            set;
        }

        [DefaultValue(20)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public uint FeeReserveCleanerNumberOfFeesToCleanEachTime
        {
            get;
            set;
        }
    }

}
