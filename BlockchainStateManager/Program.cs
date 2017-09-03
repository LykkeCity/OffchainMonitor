using Autofac;
using BlockchainStateManager.DB;
using BlockchainStateManager.Helpers;
using BlockchainStateManager.Helpers.TaskHelper;
using BlockchainStateManager.Models;
using BlockchainStateManager.Offchain;
using BlockchainStateManager.Settings;
using BlockchainStateManager.Transactions.Responses;
using Common.Assets;
using Common.Helpers.BlockchainExplorerHelper;
using Common.Settings;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlockchainStateManager.Helper;

namespace BlockchainStateManager
{
    class Program
    {
        public static ISettingsProvider settingsProvider = null;
        public static ITransactionBroacaster transactionBroadcaster = null;
        public static IFeeManager feeManager = null;
        public static IBlockchainExplorerHelper blockchainExplorerHelper = null;
        public static IDaemonHelper daemonHelper = null;

        private static AzureStorageTaskHelper azureStorageTaskHelper = null;
        private static BitcoinTaskHelper bitcoinTaskHelper = null;
        private static QBitninjaTaskHelper qbitninjaTaskHelper = null;
        private static OffchainHelper offchainHelper = null;
        private static IISTaskHelper iisTaskHelper = null;

        private static string ASSET = "btc"; // "TestExchangeUSD"

        const int USDMULTIPLICATIONFACTOR = 100;
        // const int DESIREDASSETMULTIPLICATIONFACTOR = USDMULTIPLICATIONFACTOR;
        const long DESIREDASSETMULTIPLICATIONFACTOR = (long)Constants.BTCToSathoshiMultiplicationFactor;

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
            transactionBroadcaster = Bootstrap.container.Resolve<ITransactionBroacaster>();
            feeManager = Bootstrap.container.Resolve<IFeeManager>();
            blockchainExplorerHelper = Bootstrap.container.Resolve<IBlockchainExplorerHelper>();

            azureStorageTaskHelper = new AzureStorageTaskHelper(settingsProvider as IBlockchainStateManagerSettingsProvider);
            bitcoinTaskHelper = new BitcoinTaskHelper(settingsProvider as IBlockchainStateManagerSettingsProvider);
            qbitninjaTaskHelper = new QBitninjaTaskHelper(settingsProvider as IBlockchainStateManagerSettingsProvider);
            iisTaskHelper = new IISTaskHelper();
            offchainHelper = new OffchainHelper(blockchainExplorerHelper, settingsProvider as IBlockchainStateManagerSettingsProvider);

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

            var multisig = GetMultiSigFromTwoPubKeys(clientPrivateKey.PubKey, hubPrivateKey.PubKey);

            var coloredRPC = GetColoredRPCClient(settings as IBlockchainStateManagerSettings);

            BitcoinColoredAddress[] addresses = new BitcoinColoredAddress[3];
            addresses[0] = clientPrivateKey.PubKey.WitHash.ScriptPubKey.GetScriptAddress(settings.Network).ToColoredAddress(); // c74X52L89exgnqwrFmeKzZTAxi49sLxqoNS
            addresses[1] = hubPrivateKey.PubKey.WitHash.ScriptPubKey.GetScriptAddress(settings.Network).ToColoredAddress(); // c7Acx1CTvvwPr9kEa91roJDeNLjB1evkXWG
            addresses[2] = BitcoinAddress.Create(multisig.SegwitMultiSigAddress).ToColoredAddress(); // c7GuJuSS5aNg9s791bDk6RgSpYCge3kaiHn

            long[] valuesToSend = new long[3];
            valuesToSend[0] = 100 * DESIREDASSETMULTIPLICATIONFACTOR;
            valuesToSend[1] = 100 * DESIREDASSETMULTIPLICATIONFACTOR;
            valuesToSend[2] = 85 * DESIREDASSETMULTIPLICATIONFACTOR;

            uint256[] txIds = new uint256[3];

            for (int i = 0; i < 3; i++)
            {
                if (ASSET.ToLower().Trim() == "btc")
                {
                    txIds[i] = await daemonHelper.SendBitcoinToDestination(addresses[i].Address, valuesToSend[i]);
                }
                else
                {
                    txIds[i] = await coloredRPC.IssueAssetAsync(Constants.USDAssetPrivateKey.GetAddress(), addresses[i], valuesToSend[i]);
                }
            }

            var bitcoinRPCCLient = GetRPCClient(settings as IBlockchainStateManagerSettings);
            var blkIds = await bitcoinRPCCLient.GenerateBlocksAsync(1);
            await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                (blockchainExplorerHelper.HasBlockIndexed, blkIds);

            for (int i = 0; i < txIds.Count(); i++)
            {
                await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                    (blockchainExplorerHelper.HasTransactionIndexed, new string[] { txIds[i].ToString() });
                await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed(blockchainExplorerHelper.HasBalanceIndexed,
                    new string[] { txIds[i].ToString() }, addresses[i].Address.ToString());
            }

            var unsignedChannelsetup = await offchainHelper.GenerateUnsignedChannelSetupTransaction
                (clientPrivateKey.PubKey, 10, hubPrivateKey.PubKey, 10, ASSET, 7);

            var clientSignedTx = await Helper.SignTransactionWorker(new TransactionSignRequest
            {
                TransactionToSign = unsignedChannelsetup.UnsigndTransaction,
                PrivateKey = clientPrivateKey.ToString()
            });

            var unsignedResp = await offchainHelper.CreateUnsignedClientCommitmentTransaction
                (unsignedChannelsetup.UnsigndTransaction, clientSignedTx, 30, 75, clientPrivateKey.PubKey,
                 hubPrivateKey.ToString(), ASSET, hubSelfRevokKey.PubKey, 144);

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

        private static async Task<bool> StartRequiredJobs(bool resetEverything = false)
        {
            if (!iisTaskHelper.Stop())
            {
                return false;
            }

            if (resetEverything)
            {
                if (!azureStorageTaskHelper.ClearAzureTables())
                {
                    return false;
                }

                if (!bitcoinTaskHelper.EmptyBitcoinDirectiry())
                {
                    return false;
                }
            }

            if (!await bitcoinTaskHelper.StartClearVersionOfBitcoinRegtest(resetEverything))
            {
                return false;
            }

            if (!await qbitninjaTaskHelper.StartQBitNinjaListener())
            {
                return false;
            }

            if (!iisTaskHelper.Start())
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

        private async static Task<bool> VerifyTransaction(Transaction tx)
        {
            // Block is just for testing, should be removed, after tests pass
            TransactionBuilder builder = new TransactionBuilder();
            for (int i = 0; i < tx.Inputs.Count; i++)
            {
                var coin = new Coin(new Transaction(await blockchainExplorerHelper.GetTransactionHex(tx.Inputs[i].PrevOut.Hash.ToString())), tx.Inputs[i].PrevOut.N);
                builder.AddCoins(coin);
            }
            return builder.Verify(tx);
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
                var feeSourcePrivateKey = new BitcoinSecret(new Key(), settings.Network);
                var feeDestinationPrivateKey = new BitcoinSecret(new Key(), settings.Network);
                uint feeCount = 100;

                AssetDefinition usdAsset = null;
                foreach (var item in (settings as IBlockchainStateManagerSettings).Assets)
                {
                    if (item.Name == "TestExchangeUSD")
                    {
                        usdAsset = item;
                        break;
                    }
                }

                if (!await StartRequiredJobs(true))
                {
                    return false;
                }

                var bitcoinRPCCLient = GetRPCClient(settings as IBlockchainStateManagerSettings);

                IEnumerable<string> blkIds = null;

                for (int i = 0; i < 201; i++)
                {
                    var blkCount = 1;

                    blkIds = await bitcoinRPCCLient.GenerateBlocksAsync(blkCount);
                    await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                        (blockchainExplorerHelper.HasBlockIndexed, blkIds);
                }

                /*
                for (int i = 0; i < 11; i++)
                {
                    var blkCount = 20;
                    if (i == 10)
                    {
                        blkCount = 1;
                    }

                    blkIds = await bitcoinRPCCLient.GenerateBlocksAsync(blkCount);
                    await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                        (blockchainExplorerHelper.HasBlockIndexed, blkIds);
                }
                */
                await bitcoinRPCCLient.ImportPrivKeyAsync(new BitcoinSecret(usdAsset.PrivateKey));

                var txId = await bitcoinRPCCLient.SendToAddressAsync(new BitcoinSecret(usdAsset.PrivateKey).GetAddress(),
                    new Money(100 * Constants.BTCToSathoshiMultiplicationFactor));
                await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                    (blockchainExplorerHelper.HasTransactionIndexed, new string[] { txId.ToString() });

                txId = await bitcoinRPCCLient.SendToAddressAsync(feeSourcePrivateKey.GetAddress(),
                    new Money((feeCount + 1) * Constants.BTCToSathoshiMultiplicationFactor));

                blkIds = await bitcoinRPCCLient.GenerateBlocksAsync(1);
                await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                    (blockchainExplorerHelper.HasBlockIndexed, blkIds);

                await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                    (blockchainExplorerHelper.HasTransactionIndexed, new string[] { txId.ToString() });
                await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                    (blockchainExplorerHelper.HasBalanceIndexed, new string[] { txId.ToString() }, feeSourcePrivateKey.GetAddress().ToString());

                var error = await feeManager.GenerateFees(feeSourcePrivateKey, feeDestinationPrivateKey, (int)feeCount);
                if (error != null)
                {
                    return false;
                }

                var signedResp = await GetOffchainSignedSetup(privateKeys);

                await transactionBroadcaster.BroadcastTransactionToBlockchain
                    (signedResp.FullySignedSetupTransaction);
                await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                    (blockchainExplorerHelper.HasTransactionIndexed, new string[] { new Transaction(signedResp.FullySignedSetupTransaction).GetHash().ToString() });

                /*
                 * // Not used
                var unsignedCommitment = await offchainHelper.CreateUnsignedCommitmentTransactions(signedResp.FullySignedSetupTransaction, clientPrivateKey.PubKey,
                    hubPrivateKey.PubKey, 40, 65, ASSET, hubSelfRevokKey.PubKey, 10, true);
                */

                var clientSignedCommitment = await Helper.SignTransactionWorker(new TransactionSignRequest
                {
                    TransactionToSign = signedResp.UnsignedClientCommitment0,
                    PrivateKey = clientPrivateKey.ToString()
                }, SigHash.All | SigHash.AnyoneCanPay);
                var commitmentToLog = clientSignedCommitment;
                /*
                var commitmentSpendingResp = await offchainHelper.CreateCommitmentSpendingTransactionForTimeActivatePart(txSendingResult.ToHex(), hubPrivateKey.ToString(),
                    clientPrivateKey.PubKey, hubPrivateKey.PubKey, "TestExchangeUSD", hubSelfRevokKey.PubKey, 144, true);
                */
                var commitmentSpendingResp = await offchainHelper.CreateCommitmentSpendingTransactionForMultisigPart(clientSignedCommitment, clientPrivateKey.PubKey, hubPrivateKey.PubKey,
                    ASSET, 75, hubSelfRevokKey.PubKey, 144, true, clientPrivateKey, hubSelfRevokKey);

                //  Fee is now directly paid from output for bitcoin
                //  txSendingResult = await AddEnoughFeesToTransaction
                //       (new Transaction(commitmentSpendingResp.TransactionHex));

                var hubSignedCommitment = await Helper.SignTransactionWorker(new TransactionSignRequest
                {
                    TransactionToSign = clientSignedCommitment,
                    PrivateKey = hubPrivateKey.ToString()
                }, SigHash.All | SigHash.AnyoneCanPay);

                // var txSendingResult = await AddEnoughFeesToTransaction
                // (new Transaction(hubSignedCommitment));
                //var txSendingResult = new Transaction(hubSignedCommitment);
                //await transactionBroadcaster.BroadcastTransactionToBlockchain
                //    (txSendingResult.ToHex());
                //await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                //    (blockchainExplorerHelper.HasTransactionIndexed, new string[] { txSendingResult.GetHash().ToString() });
                //await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed
                //    (blockchainExplorerHelper.HasBalanceIndexedZeroConfirmation, new string[] { txSendingResult.GetHash().ToString() },
                //    clientPrivateKey.PubKey.WitHash.ScriptPubKey.GetScriptAddress(settings.Network).ToString());
                

                // bool v = await VerifyTransaction(txSendingResult);
                var punishmentToLog = commitmentSpendingResp.TransactionHex;

                using (StreamWriter writer = new StreamWriter("log.txt"))
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("HubUnsignedCommitment:");
                    builder.AppendLine(commitmentToLog);

                    builder.AppendLine("HubSignedCommitment");
                    builder.AppendLine(hubSignedCommitment);

                    builder.AppendLine("Punishment:");
                    builder.AppendLine(punishmentToLog);

                    await writer.WriteAsync(builder.ToString());
                }

                return true;
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
                    txToBeSent.AddInput(new TxIn(new OutPoint(new uint256(item.TransactionId), item.OutputNumber)));

                    TransactionSignRequest feePrivateKeySignRequest = new TransactionSignRequest
                    {
                        PrivateKey = item.PrivateKey.ToString(),
                        TransactionToSign = txToBeSent.ToHex()
                    };
                    var feeSignedTransaction = await SignTransactionWorker(feePrivateKeySignRequest,
                        SigHash.All | SigHash.AnyoneCanPay);

                    txToBeSent = new Transaction(feeSignedTransaction);
                }

                return txToBeSent;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }
    }
}
