using BlockchainStateManager.Settings;
using Common.Helpers.BlockchainExplorerHelper;
using Common.Settings;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Helpers
{
    public class BitcoinDaemonTransactionBroadcaster : ITransactionBroacaster
    {
        IBlockchainStateManagerSettingsProvider settingsProvider = null;
        IDaemonHelper daemonHelper = null;
        IBlockchainExplorerHelper blockchainExplorerHelper = null;

        public BitcoinDaemonTransactionBroadcaster(IBlockchainStateManagerSettingsProvider _settingsProvider, IDaemonHelper _daemonHelper,
            IBlockchainExplorerHelper _blockchainExplorerHelper)
        {
            settingsProvider = _settingsProvider;
            daemonHelper = _daemonHelper;
            blockchainExplorerHelper = _blockchainExplorerHelper;
        }

        public async Task BroadcastTransactionToBlockchain(string transaction)
        {
            var settings = settingsProvider.GetSettings();

            var tx = new Transaction(transaction);
            var rpcClient = Helper.GetRPCClient(settings);
            await rpcClient.SendRawTransactionAsync(tx);
            await daemonHelper.GenerateBlocks(1);
            await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed(blockchainExplorerHelper.HasTransactionIndexed,
                new string[] { tx.GetHash().ToString() }, null);
        }
    }
}
