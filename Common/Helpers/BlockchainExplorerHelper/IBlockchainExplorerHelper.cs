using Common.Assets;
using Common.Models;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Common.Helpers.BlockchainExplorerHelper
{
    public interface IBlockchainExplorerHelper
    {
        Task<bool> HasTransactionIndexed(string txId, string dummy);
        Task<bool> HasBlockIndexed(string blockId, string dummy);
        Task<bool> HasBalanceIndexed(string txId, string btcAddress);
        Task<bool> HasBalanceIndexedZeroConfirmation(string txId, string btcAddress);
        Task WaitUntillBlockchainExplorerHasIndexed(Func<string, string, Task<bool>> checkIndexed,
            IEnumerable<string> ids, string id2 = null);
        Task<string> GetTransactionHex(string transactionId);
        Task<Transaction[]> GetTransactionsHex(UniversalUnspentOutput[] outputList);
        UniversalUnspentOutput[] GetWalletOutputsUncolored(UniversalUnspentOutput[] input);
        UniversalUnspentOutput[] GetWalletOutputsForAsset(UniversalUnspentOutput[] input, string assetId);
        Task<Tuple<UniversalUnspentOutput[], bool, string>> GetWalletOutputs(string walletAddress);
        bool IsAssetsEnough(UniversalUnspentOutput[] outputs, string assetId, float assetAmount,
            long multiplyFactor, bool includeUnconfirmed = false);
        bool IsBitcoinsEnough(UniversalUnspentOutput[] outputs, long amountInSatoshi, bool includeUnconfirmed = false);
        Task<bool> IsAssetsEnough(string walletAddress, string assetId, int amount, Network network, long multiplyFactor,
            bool includeUnconfirmed = false);

        Task<Tuple<float, float, bool, string>> GetAccountBalance(string walletAddress, string assetId);
        float GetAssetBalance(UniversalUnspentOutput[] outputs, string assetId, long multiplyFactor, bool includeUnconfirmed = false);
        Task<Tuple<ColoredCoin[], Coin[]>> GetColoredUnColoredCoins(UniversalUnspentOutput[] walletOutputs, string assetId);
        Task<GetCoinsForWalletReturnType> GetCoinsForWallet(string multiSigAddress, long requiredSatoshiAmount, float requiredAssetAmount,
            string asset, AssetDefinition[] assets, string connectionString, bool isOrdinaryReturnTypeRequired, bool isAddressMultiSig = false);
    }
}
