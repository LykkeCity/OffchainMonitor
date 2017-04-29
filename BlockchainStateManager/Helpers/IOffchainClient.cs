using BlockchainStateManager.Transactions.Responses;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Helpers
{
    public interface IOffchainClient
    {
        Task<UnsignedChannelSetupTransaction> GenerateUnsignedChannelSetupTransaction(PubKey clientPubkey, double clientContributedAmount,
            PubKey hubPubkey, double hubContributedAmount, string channelAssetName, int channelTimeoutInMinutes);

        Task<UnsignedClientCommitmentTransactionResponse> CreateUnsignedClientCommitmentTransaction(string UnsignedChannelSetupTransaction,
            string ClientSignedChannelSetup, double clientCommitedAmount, double hubCommitedAmount, PubKey clientPubkey,
            string hubPrivatekey, string assetName, PubKey counterPartyRevokePubkey, int activationIn10Minutes);

        Task<CommitmentCustomOutputSpendingTransaction> CreateCommitmentSpendingTransactionForTimeActivatePart(string commitmentTransactionHex,
            BitcoinSecret spendingPrivateKey, PubKey clientPubkey, PubKey hubPubkey, string assetName,
            PubKey lockingPubkey, int activationIn10Minutes, bool clientSendsCommitmentToHub);

        Task<CreateUnsignedCommitmentTransactionsResponse> CreateUnsignedCommitmentTransactions(string signedSetupTransaction, string clientPubkey,
            string hubPubkey, double clientAmount, double hubAmount, string assetName, string lockingPubkey, int activationIn10Minutes, bool clientSendsCommitmentToHub);


    }
}
