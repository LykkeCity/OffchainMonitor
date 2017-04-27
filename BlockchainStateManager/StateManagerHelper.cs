using BlockchainStateManager.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager
{
    public class StateManagerHelper
    {
        IBlockchainExplorerHelper blockchainExplorerHelper = null;
        IDaemonHelper daemonHelper = null;
        public StateManagerHelper(IBlockchainExplorerHelper _blockchainExplorerHelper, IDaemonHelper _daemonHelper)
        {
            blockchainExplorerHelper = _blockchainExplorerHelper;
            daemonHelper = _daemonHelper;
        }
        /*
        public async Task<string> CashinToAddress(string destAddress, string asset, double amount)
        {
            CashInRequestModel cashin = new CashInRequestModel
            {
                TransactionId = Guid.NewGuid().ToString(),
                MultisigAddress = destAddress,
                Amount = amount,
                Currency = asset
            };
            var reply = await CreateLykkeWalletRequestAndProcessResult<CashInResponse>
                ("CashIn", cashin, QueueReader, QueueWriter);
            await daemonHelper.GenerateBlocks(1);
            await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed(blockchainExplorerHelper.HasTransactionIndexed,
                new string[] { reply.Result.TransactionHash }, null);
            await blockchainExplorerHelper.WaitUntillBlockchainExplorerHasIndexed(blockchainExplorerHelper.HasBalanceIndexed,
                new string[] { reply.Result.TransactionHash }, destAddress);

            return reply.TransactionId;
        }
        */
    }
}
