using Autofac;
using BlockchainStateManager.Helpers;
using BlockchainStateManager.Helpers.TaskHelper;
using BlockchainStateManager.Models;
using BlockchainStateManager.Settings;
using BlockchainStateManager.Transactions.Responses;
using NBitcoin;
using System;
using System.Threading.Tasks;
using static BlockchainStateManager.Helper;

namespace BlockchainStateManager
{
    class Program
    {
        public static ISettingsProvider settingsProvider = null;
        public static IDaemonHelper daemonHelper = null;
        public static IOffchainClient offchianClient = null;
        public static ITransactionBroacaster transactionBroadcaster = null;
        public static IFeeManager feeManager = null;

        private static AzureStorageTaskHelper azureStorageTaskHelper = null;
        private static WalletbackEndTaskHelper walletbackendTaskHelper = null;
        private static BitcoinTaskHelper bitcoinTaskHelper = null;
        private static QBitninjaTaskHelper qbitninjaTaskHelper = null;

        static Program()
        {
            Bootstrap.Start();
        }

        static void Main(string[] args)
        {
            string[] reservedPrivateKey = new string[] {
            "cQMqC1Vqyi6o62wE1Z1ZeWDbMCkRDZW5dMPJz8QT9uMKQaMZa8JY",
            "cQyt2zxAS2uV7HJWR9hf16pFDTye8YsGL6hzd9pQzMoo9m24RGoV",
            "cSFbgd8zKDSCDHgGocccngyVSfGZsyZFiTXtimTonHyL44gTKTNU",  // 03eb5b1a93a77d6743bd4657614d87f4d2d40566558d4c8faab188d957c32c1976
            "cPBtsvLrD3DnbdGgDZ2EMbZnQurzBVmgmejiMv55jH9JehPDn5Aq"   // 035441d55de4f28fcb967472a1f9790ecfea9a9a2a92e301646d52cb3290b9e355
            };

            settingsProvider = Bootstrap.container.Resolve<ISettingsProvider>();
            daemonHelper = Bootstrap.container.Resolve<IDaemonHelper>();
            offchianClient = Bootstrap.container.Resolve<IOffchainClient>();
            transactionBroadcaster = Bootstrap.container.Resolve<ITransactionBroacaster>();
            feeManager = Bootstrap.container.Resolve<IFeeManager>();

            azureStorageTaskHelper = new AzureStorageTaskHelper(settingsProvider);
            bitcoinTaskHelper = new BitcoinTaskHelper(settingsProvider);
            qbitninjaTaskHelper = new QBitninjaTaskHelper(settingsProvider);
            walletbackendTaskHelper = new WalletbackEndTaskHelper(settingsProvider);

            if(!PutBlockchainInAKnownState(reservedPrivateKey).Result)
            {
                System.Console.WriteLine("Error putting blockchain in a known state.");
            }
        }

        private static async Task<UnsignedClientCommitmentTransactionResponse> GetOffchainSignedSetup
            (string[] privateKeys)
        {
            var settings = settingsProvider.GetSettings();

            var clientPrivateKey = new BitcoinSecret(privateKeys[0]);
            var hubPrivateKey = new BitcoinSecret(privateKeys[1]);
            var hubSelfRevokKey = new BitcoinSecret(privateKeys[3]);

            var multisig = GetMultiSigFromTwoPubKeys(clientPrivateKey.PubKey.ToString(),
                hubPrivateKey.PubKey.ToString());

            var coloredRPC = GetColoredRPCClient(settings);
            var assetName = "TestExchangeUSD";
            var usdAssetPrivateKey = new BitcoinSecret("cQc1KwWUg5jPZG8PC7xisJ82GSBdafpdhhNBvwSqZCcJuafX96BL");
            await coloredRPC.IssueAssetAsync(usdAssetPrivateKey.GetAddress(), clientPrivateKey.GetAddress(), 100);
            await coloredRPC.IssueAssetAsync(hubPrivateKey.GetAddress(), clientPrivateKey.GetAddress(), 100);
            await coloredRPC.IssueAssetAsync(BitcoinAddress.GetFromBase58Data(multisig.MultiSigAddress) as BitcoinAddress,
                clientPrivateKey.GetAddress(), 85);

            var unsignedChannelsetup = await offchianClient.GenerateUnsignedChannelSetupTransaction
                (clientPrivateKey.PubKey, 10, hubPrivateKey.PubKey, 10, "TestExchangeUSD", 7);

            var clientSignedTx = await Helper.SignTransactionWorker(new TransactionSignRequest
            {
                TransactionToSign = unsignedChannelsetup.UnsigndTransaction,
                PrivateKey = clientPrivateKey.ToString()
            });

            var unsignedResp = await offchianClient.CreateUnsignedClientCommitmentTransaction
                (unsignedChannelsetup.UnsigndTransaction, clientSignedTx, 30, 75, clientPrivateKey.PubKey,
                 hubPrivateKey.ToString(), "TestExchangeUSD", hubSelfRevokKey.PubKey, 144);

            return unsignedResp;
        }

        public class CommitmentPunishmentPair
        {
            public string Commitment
            {
                get;
                set;
            }

            public string Punishment
            {
                get;
                set;
            }
        }
        /*
        private static string GetUrlForCommitmentCreation(string baseUrl, string signedSetupTransaction, string clientPubkey,
            string hubPubkey, double clientAmount, double hubAmount, string assetName, string lockingPubkey, int activationIn10Minutes, bool clientSendsCommitmentToHub)
        {

            return string.Format("{0}/Offchain/CreateUnsignedCommitmentTransactions?signedSetupTransaction={1}&clientPubkey={2}&hubPubkey={3}&clientAmount={4}&hubAmount={5}&assetName={6}&lockingPubkey={7}&activationIn10Minutes={8}&clientSendsCommitmentToHub={9}",
                baseUrl, signedSetupTransaction, clientPubkey, hubPubkey, clientAmount, hubAmount, assetName,
                lockingPubkey, activationIn10Minutes, clientSendsCommitmentToHub);
        }
        */


        public static async Task<bool> PutBlockchainInAKnownState(string[] privateKeys)
        {
            var settings = settingsProvider.GetSettings();

            try
            {
                var clientPrivateKey = new BitcoinSecret(privateKeys[0]);
                var hubPrivateKey = new BitcoinSecret(privateKeys[1]);
                var clientSelfRevokeKey = new BitcoinSecret(privateKeys[2]);
                var hubSelfRevokKey = new BitcoinSecret(privateKeys[3]);

                if(!azureStorageTaskHelper.ClearAzureTables())
                {
                    return false;
                }

                if(!bitcoinTaskHelper.EmptyBitcoinDirectiry())
                {
                    return false;
                }

                if(!await bitcoinTaskHelper.StartClearVersionOfBitcoinRegtest())
                {
                    return false;
                }

                return true;

                var signedResp = await GetOffchainSignedSetup(privateKeys);
                await transactionBroadcaster.BroadcastTransactionToBlockchain
                    (signedResp.FullySignedSetupTransaction);

                var unsignedCommitment = await offchianClient.CreateUnsignedCommitmentTransactions(signedResp.FullySignedSetupTransaction, clientPrivateKey.PubKey.ToHex(),
                    hubPrivateKey.PubKey.ToHex(), 40, 65, "TestExchangeUSD", clientPrivateKey.PubKey.ToHex(), 10, false);
                
                var clientSignedCommitment = await Helper.SignTransactionWorker(new TransactionSignRequest
                {
                    TransactionToSign = signedResp.UnsignedClientCommitment0,
                    PrivateKey = clientPrivateKey.ToString()
                }, SigHash.All | SigHash.AnyoneCanPay);

                var hubSignedCommitment = await Helper.SignTransactionWorker(new TransactionSignRequest
                {
                    TransactionToSign = clientSignedCommitment,
                    PrivateKey = hubPrivateKey.ToString()
                }, SigHash.All | SigHash.AnyoneCanPay);

                var txSendingResult = await AddEnoughFeesToTransaction
                    (new Transaction(hubSignedCommitment));

                var commitmentSpendingResp = await offchianClient.CreateCommitmentSpendingTransactionForTimeActivatePart(txSendingResult.ToHex(), hubPrivateKey,
                    clientPrivateKey.PubKey, hubPrivateKey.PubKey, "TestExchangeUSD", hubSelfRevokKey.PubKey, 144, true);


            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        public static async Task<Transaction> AddEnoughFeesToTransaction
            (Transaction tx)
        {
            Transaction txToBeSent = null;

            var settings = settingsProvider.GetSettings();

            try
            {
                var fees = await feeManager.GetFeeCoinsToAddToTransaction(tx);

                txToBeSent = tx;
                foreach (var item in fees)
                {
                    txToBeSent.AddInput(new TxIn(item.Outpoint));

                    TransactionSignRequest feePrivateKeySignRequest = new TransactionSignRequest
                    {
                        PrivateKey = item.Secret.ToString(),
                        TransactionToSign = txToBeSent.ToHex()
                    };
                    var feeSignedTransaction = await SignTransactionWorker(feePrivateKeySignRequest,
                        SigHash.All | SigHash.AnyoneCanPay);

                    txToBeSent = new Transaction(feeSignedTransaction);
                }

                var rpcClient = GetRPCClient(settings);
                await rpcClient.SendRawTransactionAsync(txToBeSent);

                return tx;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }
    }
}
