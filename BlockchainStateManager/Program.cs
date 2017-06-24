using Autofac;
using BlockchainStateManager.DB;
using BlockchainStateManager.Helpers;
using BlockchainStateManager.Helpers.TaskHelper;
using BlockchainStateManager.Models;
using BlockchainStateManager.Settings;
using BlockchainStateManager.Transactions.Responses;
using NBitcoin;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using static BlockchainStateManager.Helper;
using System.Linq;

namespace BlockchainStateManager
{
    class Program
    {
        public static ISettingsProvider settingsProvider = null;
        public static IDaemonHelper daemonHelper = null;
        public static IOffchainClient offchianClient = null;
        public static ITransactionBroacaster transactionBroadcaster = null;
        public static IFeeManager feeManager = null;
        public static IBlockchainExplorerHelper blockchainExplorerHelper = null;

        private static AzureStorageTaskHelper azureStorageTaskHelper = null;
        private static WalletbackEndTaskHelper walletbackendTaskHelper = null;
        private static BitcoinTaskHelper bitcoinTaskHelper = null;
        private static QBitninjaTaskHelper qbitninjaTaskHelper = null;

        
        const int USDMULTIPLICATIONFACTOR = 100;

        static Program()
        {
            Bootstrap.Start();
            Database.SetInitializer(new DropCreateDatabaseAlways<BlockchainStateManagerContext>());
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
            blockchainExplorerHelper = Bootstrap.container.Resolve<IBlockchainExplorerHelper>();

            azureStorageTaskHelper = new AzureStorageTaskHelper(settingsProvider);
            bitcoinTaskHelper = new BitcoinTaskHelper(settingsProvider);
            qbitninjaTaskHelper = new QBitninjaTaskHelper(settingsProvider);
            walletbackendTaskHelper = new WalletbackEndTaskHelper(settingsProvider);

            if (!PutBlockchainInAKnownState(reservedPrivateKey).Result)
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
            //var txId = await coloredRPC.IssueAssetAsync(usdAssetPrivateKey.GetAddress(), clientPrivateKey.GetAddress().ToColoredAddress(), 8500);         
            await coloredRPC.IssueAssetAsync(Constants.USDAssetPrivateKey.GetAddress(), clientPrivateKey.GetAddress().ToColoredAddress(), 100 * USDMULTIPLICATIONFACTOR);
            await coloredRPC.IssueAssetAsync(Constants.USDAssetPrivateKey.GetAddress(), hubPrivateKey.GetAddress().ToColoredAddress(), 100 * USDMULTIPLICATIONFACTOR);
            await coloredRPC.IssueAssetAsync(Constants.USDAssetPrivateKey.GetAddress(), (BitcoinAddress.GetFromBase58Data(multisig.MultiSigAddress) as BitcoinAddress).ToColoredAddress(),
                85 * USDMULTIPLICATIONFACTOR);

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

        private static async Task<bool> StartRequiredJobs()
        {
            if (!azureStorageTaskHelper.ClearAzureTables())
            {
                return false;
            }

            if (!bitcoinTaskHelper.EmptyBitcoinDirectiry())
            {
                return false;
            }

            if (!await bitcoinTaskHelper.StartClearVersionOfBitcoinRegtest())
            {
                return false;
            }

            if (!await qbitninjaTaskHelper.StartQBitNinjaListener())
            {
                return false;
            }

            /*
            if(!await walletbackendTaskHelper.StartWalletBackend())
            {
                return false;
            }
            */
            return true;
        }

        public static async Task<bool> PutBlockchainInAKnownState(string[] privateKeys)
        {
            var settings = settingsProvider.GetSettings();

            try
            {
                var clientPrivateKey = new BitcoinSecret(privateKeys[0]);
                var hubPrivateKey = new BitcoinSecret(privateKeys[1]);
                var clientSelfRevokeKey = new BitcoinSecret(privateKeys[2]);
                var hubSelfRevokKey = new BitcoinSecret(privateKeys[3]);
                var usdPrivateKey = new BitcoinSecret("cQc1KwWUg5jPZG8PC7xisJ82GSBdafpdhhNBvwSqZCcJuafX96BL");
                var feeSourcePrivateKey = new BitcoinSecret(new Key(), settings.Network);
                var feeDestinationPrivateKey = new BitcoinSecret(new Key(), settings.Network);
                uint feeCount = 100;

                if (!await StartRequiredJobs())
                {
                    return false;
                }

                var bitcoinRPCCLient = GetRPCClient(settings);
                var blkIds = await bitcoinRPCCLient.GenerateBlocksAsync(201);
                await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                    (blockchainExplorerHelper.HasBlockIndexed, blkIds);
                await bitcoinRPCCLient.ImportPrivKeyAsync(usdPrivateKey);

                var txId = await bitcoinRPCCLient.SendToAddressAsync(usdPrivateKey.GetAddress(),
                    new Money(100 * Constants.BTCToSathoshiMultiplicationFactor));
                await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                    (blockchainExplorerHelper.HasTransactionIndexed, new string[] { txId.ToString() });

                txId = await bitcoinRPCCLient.SendToAddressAsync(feeSourcePrivateKey.GetAddress(),
                    new Money((feeCount + 1) * Constants.BTCToSathoshiMultiplicationFactor));
                await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                    (blockchainExplorerHelper.HasTransactionIndexed, new string[] { txId.ToString() });
                await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                    (blockchainExplorerHelper.HasBalanceIndexedZeroConfirmation, new string[] { txId.ToString()}, feeSourcePrivateKey.GetAddress().ToWif() );

                blkIds = await bitcoinRPCCLient.GenerateBlocksAsync(1);
                await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                    (blockchainExplorerHelper.HasBlockIndexed, blkIds);

                var error = await feeManager.GenerateFees(feeSourcePrivateKey, feeDestinationPrivateKey, (int)feeCount);
                if (error != null)
                {
                    return false;
                }


                var signedResp = await GetOffchainSignedSetup(privateKeys);


                return true;
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
