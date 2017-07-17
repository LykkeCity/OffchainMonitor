using Autofac;
using BlockchainStateManager.Helpers;
using BlockchainStateManager.Models;
using BlockchainStateManager.Settings;
using Common.Settings;
using NBitcoin;
using NBitcoin.OpenAsset;
using NBitcoin.RPC;
using QBitNinja.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlockchainStateManager
{
    public class Helper
    {
        static IDaemonHelper daemonHelper = null;
        static ISettingsProvider settingsProvider = null;

        static Helper()
        {
            daemonHelper = Bootstrap.container.Resolve<IDaemonHelper>();
            settingsProvider = Bootstrap.container.Resolve<ISettingsProvider>();
        }

        private static Transaction SignWithSinglePrivateKey(Transaction[] previousTransactions, Transaction tx,
            BitcoinSecret secret, SigHash sigHash)
        {
            var secretSegWitScriptPubKey = secret.PubKey.WitHash.ScriptPubKey;

            TransactionBuilder builder = new TransactionBuilder();
            builder.ContinueToBuild(tx);
            for (int i = 0; i < previousTransactions.Count(); i++)
            {
                var prevOut = previousTransactions[i].Outputs[tx.Inputs[i].PrevOut.N];
                var bearer = new Coin(previousTransactions[i], tx.Inputs[i].PrevOut.N);
                if (prevOut.ScriptPubKey == secretSegWitScriptPubKey)
                {
                    new ScriptCoin(bearer, secretSegWitScriptPubKey);
                }
                else
                {
                    builder.AddCoins(bearer);
                }
            }
            tx = builder.AddKeys(new BitcoinSecret[] { secret }).SignTransaction(tx, sigHash);

            return tx;
        }

        private static async Task<Transaction[]> GetPreviousTransactions(Transaction tx)
        {
            Transaction[] previousTransactions = null;
            {
                previousTransactions = new Transaction[tx.Inputs.Count];
                for (int i = 0; i < previousTransactions.Count(); i++)
                {
                    // var txResponse = await GetTransactionHex(tx.Inputs[i].PrevOut.Hash.ToString(), WebSettings.ConnectionParams);
                    var txResponse = await daemonHelper.GetTransactionHex
                        (tx.Inputs[i].PrevOut.Hash.ToString());

                    if (txResponse.Item1)
                    {
                        throw new Exception(string.Format("Error while retrieving transaction {0}, error is: {1}",
                            tx.Inputs[i].PrevOut.Hash.ToString(), txResponse.Item2));
                    }

                    previousTransactions[i] = new Transaction(txResponse.Item3);
                }
            }

            return previousTransactions;
        }

        public static async Task<string> SignTransactionWorker(TransactionSignRequest signRequest,
            SigHash sigHash = SigHash.All)
        {
            var settings = settingsProvider.GetSettings();

            Transaction tx = new Transaction(signRequest.TransactionToSign);
            Transaction outputTx = new Transaction(signRequest.TransactionToSign);
            var secret = new BitcoinSecret(signRequest.PrivateKey);
            var secretSegWitScriptPubKey = secret.PubKey.WitHash.ScriptPubKey;
            var previousTransactions = await GetPreviousTransactions(tx);

            tx = SignWithSinglePrivateKey(previousTransactions, tx, secret, sigHash);

            for (int i = 0; i < tx.Inputs.Count; i++)
            {
                var input = tx.Inputs[i];

                var prevTransaction = previousTransactions[i];
                var output = prevTransaction.Outputs[input.PrevOut.N];

                if (PayToScriptHashTemplate.Instance.CheckScriptPubKey(output.ScriptPubKey))
                {
                    var redeemScript = PayToScriptHashTemplate.Instance.ExtractScriptSigParameters(input.ScriptSig).RedeemScript;
                    if (PayToMultiSigTemplate.Instance.CheckScriptPubKey(redeemScript))
                    {
                        var pubkeys = PayToMultiSigTemplate.Instance.ExtractScriptPubKeyParameters(redeemScript).PubKeys;
                        for (int j = 0; j < pubkeys.Length; j++)
                        {
                            if (secret.PubKey.ToHex() == pubkeys[j].ToHex())
                            {
                                var scriptParams = PayToScriptHashTemplate.Instance.ExtractScriptSigParameters(input.ScriptSig);
                                var hash = Script.SignatureHash(scriptParams.RedeemScript, tx, i, sigHash);
                                var signature = secret.PrivateKey.Sign(hash, sigHash);
                                scriptParams.Pushes[j + 1] = signature.Signature.ToDER().Concat(new byte[] { (byte)sigHash }).ToArray();
                                outputTx.Inputs[i].ScriptSig = PayToScriptHashTemplate.Instance.GenerateScriptSig(scriptParams);
                            }
                        }
                    }
                    continue;
                }

                if (PayToPubkeyHashTemplate.Instance.CheckScriptPubKey(output.ScriptPubKey))
                {
                    var address = PayToPubkeyHashTemplate.Instance.ExtractScriptPubKeyParameters(output.ScriptPubKey)
                        .GetAddress(settings.Network).ToWif();
                    if (address == secret.GetAddress().ToWif())
                    {
                        var hash = Script.SignatureHash(output.ScriptPubKey, tx, i, sigHash);
                        var signature = secret.PrivateKey.Sign(hash, sigHash);

                        outputTx.Inputs[i].ScriptSig = PayToPubkeyHashTemplate.Instance.GenerateScriptSig(signature, secret.PubKey);
                    }

                    continue;
                }
            }

            return outputTx.ToHex();
        }

        public static Multisig GetMultiSigFromTwoPubKeys(PubKey clientPubkey, PubKey hubPubkey)
        {
            var settings = settingsProvider.GetSettings();
            var network = settings.Network;

            var multiSigAddress = PayToMultiSigTemplate.Instance.GenerateScriptPubKey(2, new PubKey[] { clientPubkey ,
                hubPubkey });

            var retValue = new Multisig();
            retValue.MultiSigAddress = multiSigAddress.WitHash.ScriptPubKey.GetScriptAddress(settings.Network).ToString();
            retValue.MultiSigScript = multiSigAddress.ToString();
            retValue.WalletAddress = clientPubkey.WitHash.ScriptPubKey.GetScriptAddress(network).ToString();
            return retValue;
        }

        public static Multisig GetMultiSigFromTwoPubKeys(string clientPubkey, string hubPubkey)
        {
            return GetMultiSigFromTwoPubKeys(new PubKey(clientPubkey), new PubKey(hubPubkey));
        }

        /*
        public Asset GetAssetId(string assetName)
        {
            var settings = settingsProvider.GetSettings();
            
            switch(assetName)
            {
                case "TestExchangeUSD":
                    return new Asset
                    {
                        AssetId = new AssetId(Constants.USDAssetPrivateKey.GetAddress().ScriptPubKey).ToString(settings.Network),
                        MultiplicationFactor = Constants.USDAssetMultiplicationFactor
                    };
                default:
                    return null;
            }
        }
        */
        /*
        public static async Task<Tuple<bool, string, string>> GetTransactionHex(string transactionId,
            RPCConnectionParams connectionParams)
        {
            string transactionHex = "";
            bool errorOccured = false;
            string errorMessage = "";
            try
            {
                RPCClient client = new RPCClient(new System.Net.NetworkCredential(connectionParams.Username, connectionParams.Password),
                                connectionParams.IpAddress, connectionParams.BitcoinNetwork);
                transactionHex = (await client.GetRawTransactionAsync(uint256.Parse(transactionId), true)).ToHex();
            }
            catch (Exception e)
            {
                errorOccured = true;
                errorMessage = e.ToString();
            }
            return new Tuple<bool, string, string>(errorOccured, errorMessage, transactionHex);
        }
        */

        public static NColorCore.RPC.RPCClient GetColoredRPCClient(IBlockchainStateManagerSettings setting)
        {
            UriBuilder builder = new UriBuilder();
            builder.Host = setting.RegtestRPCIP;
            builder.Scheme = "http";
            builder.Port = setting.ColorCorePort;
            var uri = builder.Uri;

            return new NColorCore.RPC.RPCClient(new System.Net.NetworkCredential(setting.RegtestRPCUsername, setting.RegtestRPCPassword),
                uri);
        }

        public static LykkeExtenddedRPCClient GetRPCClient(IBlockchainStateManagerSettings setting)
        {
            UriBuilder builder = new UriBuilder();
            builder.Host = setting.RegtestRPCIP;
            builder.Scheme = "http";
            builder.Port = setting.RegtestPort;
            var uri = builder.Uri;

            return new LykkeExtenddedRPCClient(new System.Net.NetworkCredential(setting.RegtestRPCUsername, setting.RegtestRPCPassword),
                uri);
        }

        // From: http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
