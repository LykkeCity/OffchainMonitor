using BlockchainStateManager.Settings;
using BlockchainStateManager.Transactions.Responses;
using LykkeWalletServices.Transactions.Responses;
using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Helpers
{
    public class WalletBackendOffchainClient : IOffchainClient
    {
        ISettingsProvider settingsProvider;

        public WalletBackendOffchainClient(ISettingsProvider _settingsProvider)
        {
            settingsProvider = _settingsProvider;
        }

        public async Task<UnsignedChannelSetupTransaction> GenerateUnsignedChannelSetupTransaction(PubKey clientPubkey, double clientContributedAmount,
            PubKey hubPubkey, double hubContributedAmount, string channelAssetName, int channelTimeoutInMinutes)
        {
            var settings = settingsProvider.GetSettings();
            using (HttpClient client = new HttpClient())
            {
                string url = string.Format("{0}/Offchain/GenerateUnsignedChannelSetupTransaction?clientPubkey={1}&clientContributedAmount={2}&hubPubkey={3}&hubContributedAmount={4}&channelAssetName={5}&channelTimeoutInMinutes={6}",
                    settings.WalletBackendUrl, clientPubkey, clientContributedAmount, hubPubkey, hubContributedAmount, channelAssetName, channelTimeoutInMinutes);
                var response = await client.GetStringAsync(url);
                UnsignedChannelSetupTransaction resp =
                    JsonConvert.DeserializeObject<UnsignedChannelSetupTransaction>(response);

                return resp;
            }
        }

        public async Task<UnsignedClientCommitmentTransactionResponse> CreateUnsignedClientCommitmentTransaction(string UnsignedChannelSetupTransaction,
            string ClientSignedChannelSetup, double clientCommitedAmount, double hubCommitedAmount, PubKey clientPubkey,
            string hubPrivatekey, string assetName, PubKey counterPartyRevokePubkey, int activationIn10Minutes)
        {
            var settings = settingsProvider.GetSettings();

            var url = string.Format("{0}/Offchain/CreateUnsignedClientCommitmentTransaction?UnsignedChannelSetupTransaction={1}&ClientSignedChannelSetup={2}&clientCommitedAmount={3}&hubCommitedAmount={4}&clientPubkey={5}&hubPrivatekey={6}&assetName={7}&counterPartyRevokePubkey={8}&activationIn10Minutes={9}",
                    settings.WalletBackendUrl, UnsignedChannelSetupTransaction, ClientSignedChannelSetup, clientCommitedAmount,
                    hubCommitedAmount, clientPubkey, hubPrivatekey, assetName, counterPartyRevokePubkey, activationIn10Minutes);

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);
                var signedResp = JsonConvert.DeserializeObject<UnsignedClientCommitmentTransactionResponse>(response);

                return signedResp;
            }
        }

        public async Task<CommitmentCustomOutputSpendingTransaction> CreateCommitmentSpendingTransactionForTimeActivatePart(string commitmentTransactionHex,
            BitcoinSecret spendingPrivateKey, PubKey clientPubkey, PubKey hubPubkey, string assetName,
            PubKey lockingPubkey, int activationIn10Minutes, bool clientSendsCommitmentToHub)
        {
            var settings = settingsProvider.GetSettings();

            var url = string.Format("{0}/Offchain/CreateCommitmentSpendingTransactionForTimeActivatePart?commitmentTransactionHex={1}&spendingPrivateKey={2}&clientPubkey={3}&hubPubkey={4}&assetName={5}&lockingPubkey={6}&activationIn10Minutes={7}&clientSendsCommitmentToHub={8}",
                    settings.WalletBackendUrl, commitmentTransactionHex, spendingPrivateKey, clientPubkey, hubPubkey, "TestExchangeUSD",
                    lockingPubkey, activationIn10Minutes, clientSendsCommitmentToHub);

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);
                var commitmentSpendingResp = JsonConvert.DeserializeObject<CommitmentCustomOutputSpendingTransaction>
                    (response);

                return commitmentSpendingResp;
            }
        }
    }
}
