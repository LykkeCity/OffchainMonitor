using BlockchainStateManager.Assets;
using BlockchainStateManager.Extensions;
using BlockchainStateManager.Models;
using BlockchainStateManager.Settings;
using NBitcoin;
using NBitcoin.OpenAsset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Helpers
{
    public abstract class AbstractBlockchainExplorerHelper : IBlockchainExplorerHelper
    {
        protected ISettingsProvider SettingsProvider
        {
            get;
            set;
        }
        public IDaemonHelper daemonHelper
        {
            get;
            set;
        }
        public AbstractBlockchainExplorerHelper(ISettingsProvider _settingsProvider)
        {
            SettingsProvider = _settingsProvider;
        }
        public abstract Task<bool> HasTransactionIndexed(string txId, string dummy);
        public abstract Task<bool> HasBlockIndexed(string blockId, string dummy);
        public abstract Task<bool> HasBalanceIndexed(string txId, string btcAddress);
        public abstract Task<bool> HasBalanceIndexedZeroConfirmation(string txId, string btcAddress);
        public abstract Task WaitUntillBlockchainExplorerHasIndexed(Func<string, string, Task<bool>> checkIndexed,
            IEnumerable<string> ids, string id2 = null);
        public abstract Task<string> GetTransactionHex(string transactionId);
        protected static ColoredCoin[] GenerateWalletColoredCoins(Transaction[] transactions, UniversalUnspentOutput[] usableOutputs,
            string assetId)
        {
            ColoredCoin[] coins = new ColoredCoin[transactions.Length];
            for (int i = 0; i < transactions.Length; i++)
            {
                coins[i] = new ColoredCoin(new AssetMoney(new AssetId(new BitcoinAssetId(assetId)), (int)usableOutputs[i].GetAssetAmount()),
                    new Coin(transactions[i], (uint)usableOutputs[i].GetOutputIndex()));
            }
            return coins;
        }

        protected static Coin[] GenerateWalletUnColoredCoins(Transaction[] transactions, UniversalUnspentOutput[] usableOutputs)
        {
            Coin[] coins = new Coin[transactions.Length];
            for (int i = 0; i < transactions.Length; i++)
            {
                coins[i] = new Coin(transactions[i], (uint)usableOutputs[i].GetOutputIndex());
            }
            return coins;
        }

        public async Task<Transaction[]> GetTransactionsHex(UniversalUnspentOutput[] outputList)
        {
            Transaction[] walletTransactions = new Transaction[outputList.Length];
            for (int i = 0; i < walletTransactions.Length; i++)
            {
                var ret = await daemonHelper.GetTransactionHex(outputList[i].GetTransactionHash());
                if (!ret.Item1)
                {
                    walletTransactions[i] = new Transaction(ret.Item3);
                }
                else
                {
                    throw new Exception("Could not get the transaction hex for the transaction with id: "
                        + outputList[i].GetTransactionHash() + " . The exact error message is " + ret.Item2);
                }
            }
            return walletTransactions;
        }

        public UniversalUnspentOutput[] GetWalletOutputsUncolored(UniversalUnspentOutput[] input)
        {
            IList<UniversalUnspentOutput> outputs = new List<UniversalUnspentOutput>();
            foreach (var item in input)
            {
                if (item.GetAssetId() == null)
                {
                    outputs.Add(item);
                }
            }

            return outputs.ToArray();
        }

        public UniversalUnspentOutput[] GetWalletOutputsForAsset(UniversalUnspentOutput[] input, string assetId)
        {
            IList<UniversalUnspentOutput> outputs = new List<UniversalUnspentOutput>();
            if (assetId != null)
            {
                foreach (var item in input)
                {
                    if (item.GetAssetId() == assetId)
                    {
                        outputs.Add(item);
                    }
                }
            }

            return outputs.ToArray();
        }
        public abstract Task<Tuple<UniversalUnspentOutput[], bool, string>> GetWalletOutputs(string walletAddress);

        public bool IsAssetsEnough(UniversalUnspentOutput[] outputs,
            string assetId, float assetAmount, long multiplyFactor, bool includeUnconfirmed = false)
        {
            if (!string.IsNullOrEmpty(assetId))
            {
                float total = GetAssetBalance(outputs, assetId, multiplyFactor, includeUnconfirmed);
                if (total >= assetAmount)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        // ToDo - Clear confirmation number
        public bool IsBitcoinsEnough(UniversalUnspentOutput[] outputs,
            long amountInSatoshi, bool includeUnconfirmed = false)
        {
            long total = 0;
            foreach (var item in outputs)
            {
                if (item.GetConfirmationNumber() == 0)
                {
                    if (includeUnconfirmed)
                    {
                        total += item.GetBitcoinAmount();
                    }
                }
                else
                {
                    total += item.GetBitcoinAmount();
                }
            }

            if (total >= amountInSatoshi)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether the amount for assetId of the wallet is enough
        /// </summary>
        /// <param name="walletAddress">Address of the wallet</param>
        /// <param name="assetId">Asset id to check the balance for.</param>
        /// <param name="amount">The required amount to check for.</param>
        /// <returns>Whether the asset amount is enough or not.</returns>
        /// ToDo - Figure out a method for unconfirmed balance
        public async Task<bool> IsAssetsEnough(string walletAddress, string assetId,
            int amount, Network network, long multiplyFactor, bool includeUnconfirmed = false)
        {
            Tuple<float, float, bool, string> result = await GetAccountBalance(walletAddress, assetId);
            if (result.Item3 == true)
            {
                return false;
            }
            else
            {
                if (!includeUnconfirmed)
                {
                    if (result.Item1 >= amount)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (result.Item1 + result.Item2 >= amount)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        public abstract Task<Tuple<float, float, bool, string>> GetAccountBalance(string walletAddress,
            string assetId);
        public float GetAssetBalance(UniversalUnspentOutput[] outputs, string assetId, long multiplyFactor, bool includeUnconfirmed = false)
        {
            float total = 0;
            foreach (var item in outputs)
            {
                if ((item.GetAssetId() != null && item.GetAssetId().Equals(assetId))
                    || (item.GetAssetId() == null && assetId.Trim().ToUpper().Equals("BTC")))
                {
                    if (item.GetConfirmationNumber() == 0)
                    {
                        if (includeUnconfirmed)
                        {
                            if (item.GetAssetId() != null)
                            {
                                total += (float)item.GetAssetAmount();
                            }
                            else
                            {
                                total += item.GetBitcoinAmount();
                            }
                        }
                    }
                    else
                    {
                        if (item.GetAssetId() != null)
                        {
                            total += (float)item.GetAssetAmount();
                        }
                        else
                        {
                            total += item.GetBitcoinAmount();
                        }
                    }
                }
            }

            return total / multiplyFactor;
        }
        public async Task<GetCoinsForWalletReturnType> GetCoinsForWallet(string multiSigAddress, long requiredSatoshiAmount, float requiredAssetAmount,
            string asset, AssetDefinition[] assets, string connectionString, bool isOrdinaryReturnTypeRequired, bool isAddressMultiSig = false)
        {
            var settings = SettingsProvider.GetSettings();

            GetCoinsForWalletReturnType ret;
            if (isOrdinaryReturnTypeRequired)
            {
                ret = new GetOrdinaryCoinsForWalletReturnType();
            }
            else
            {
                ret = new GetScriptCoinsForWalletReturnType();
            }

            try
            {
                if (isAddressMultiSig)
                {
                    // ret.MatchingAddress = await GetMatchingMultisigAddress(multiSigAddress);
                }

                // Getting wallet outputs
                var walletOutputs = await GetWalletOutputs(multiSigAddress);
                if (walletOutputs.Item2)
                {
                    ret.Error = new Error.Error();
                    ret.Error.Code = Error.ErrorCode.ProblemInRetrivingWalletOutput;
                    ret.Error.Message = walletOutputs.Item3;
                }
                else
                {
                    // Getting bitcoin outputs to provide the transaction fee
                    var bitcoinOutputs = GetWalletOutputsUncolored(walletOutputs.Item1);
                    if (!IsBitcoinsEnough(bitcoinOutputs, requiredSatoshiAmount))
                    {
                        ret.Error = new Error.Error();
                        ret.Error.Code = Error.ErrorCode.NotEnoughBitcoinAvailable;
                        ret.Error.Message = "The required amount of satoshis to send transaction is " + requiredSatoshiAmount +
                            " . The address is: " + multiSigAddress;
                    }
                    else
                    {
                        UniversalUnspentOutput[] assetOutputs = null;

                        if (Assets.Helper.IsRealAsset(asset))
                        {
                            ret.Asset = assets.GetAssetFromName(asset, settings.Network);
                            if (ret.Asset == null)
                            {
                                ret.Error = new Error.Error();
                                ret.Error.Code = Error.ErrorCode.AssetNotFound;
                                ret.Error.Message = "Could not find asset with name: " + asset;
                            }
                            else
                            {
                                // Getting the asset output to provide the assets
                                assetOutputs = GetWalletOutputsForAsset(walletOutputs.Item1, ret.Asset.AssetId);
                            }
                        }
                        if (Assets.Helper.IsRealAsset(asset) && ret.Asset != null && !IsAssetsEnough(assetOutputs, ret.Asset.AssetId, requiredAssetAmount, ret.Asset.MultiplyFactor))
                        {
                            ret.Error = new Error.Error();
                            ret.Error.Code = Error.ErrorCode.NotEnoughAssetAvailable;
                            ret.Error.Message = "The required amount of " + asset + " to send transaction is " + requiredAssetAmount +
                                " . The address is: " + multiSigAddress;
                        }
                        else
                        {
                            // Converting bitcoins to script coins so that we could sign the transaction
                            var coins = (await GetColoredUnColoredCoins(bitcoinOutputs, null)).Item2;
                            if (coins.Length != 0)
                            {
                                if (isOrdinaryReturnTypeRequired)
                                {
                                    ((GetOrdinaryCoinsForWalletReturnType)ret).Coins = coins;
                                }
                                else
                                {
                                    ((GetScriptCoinsForWalletReturnType)ret).ScriptCoins = new ScriptCoin[coins.Length];
                                    for (int i = 0; i < coins.Length; i++)
                                    {
                                        ((GetScriptCoinsForWalletReturnType)ret).ScriptCoins[i] = new ScriptCoin(coins[i], new Script(ret.MatchingAddress.MultiSigScript));
                                    }
                                }
                            }

                            if (Assets.Helper.IsRealAsset(asset))
                            {
                                // Converting assets to script coins so that we could sign the transaction
                                var assetCoins = ret.Asset != null ? (await GetColoredUnColoredCoins(assetOutputs, ret.Asset.AssetId)).Item1 : new ColoredCoin[0];

                                if (assetCoins.Length != 0)
                                {
                                    if (isOrdinaryReturnTypeRequired)
                                    {
                                        ((GetOrdinaryCoinsForWalletReturnType)ret).AssetCoins = assetCoins;
                                    }
                                    else
                                    {
                                        ((GetScriptCoinsForWalletReturnType)ret).AssetScriptCoins = new ColoredCoin[assetCoins.Length];
                                        for (int i = 0; i < assetCoins.Length; i++)
                                        {
                                            ((GetScriptCoinsForWalletReturnType)ret).AssetScriptCoins[i] = new ColoredCoin(assetCoins[i].Amount,
                                                new ScriptCoin(assetCoins[i].Bearer, new Script(ret.MatchingAddress.MultiSigScript)));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ret.Error = new Error.Error();
                ret.Error.Code = Error.ErrorCode.Exception;
                ret.Error.Message = e.ToString();
            }

            return ret;
        }

        private async Task<Transaction[]> GetTransactionsHex(UniversalUnspentOutput[] outputList, Network network,
            string username, string password, string ipAddress, int port)
        {
            Transaction[] walletTransactions = new Transaction[outputList.Length];
            for (int i = 0; i < walletTransactions.Length; i++)
            {
                var ret = await daemonHelper.GetTransactionHex(outputList[i].GetTransactionHash());
                if (!ret.Item1)
                {
                    walletTransactions[i] = new Transaction(ret.Item3);
                }
                else
                {
                    throw new Exception("Could not get the transaction hex for the transaction with id: "
                        + outputList[i].GetTransactionHash() + " . The exact error message is " + ret.Item2);
                }
            }
            return walletTransactions;
        }

        public async Task<Tuple<ColoredCoin[], Coin[]>> GetColoredUnColoredCoins(UniversalUnspentOutput[] walletOutputs, string assetId)
        {
            var walletAssetOutputs = GetWalletOutputsForAsset(walletOutputs, assetId);
            var walletUncoloredOutputs = GetWalletOutputsUncolored(walletOutputs);
            var walletColoredTransactions = await GetTransactionsHex(walletAssetOutputs);
            var walletUncoloredTransactions = await GetTransactionsHex(walletUncoloredOutputs);
            var walletColoredCoins = GenerateWalletColoredCoins(walletColoredTransactions, walletAssetOutputs, assetId);
            var walletUncoloredCoins = GenerateWalletUnColoredCoins(walletUncoloredTransactions, walletUncoloredOutputs);
            return new Tuple<ColoredCoin[], Coin[]>(walletColoredCoins, walletUncoloredCoins);
        }
    }
}
