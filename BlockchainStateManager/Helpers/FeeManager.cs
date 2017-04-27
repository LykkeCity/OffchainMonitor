using BlockchainStateManager.Models;
using BlockchainStateManager.Settings;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockchainStateManager.Models;

namespace BlockchainStateManager.Helpers
{
    public class FeeManager : IFeeManager
    {
        ITransactionBroacaster transactionBroadcaster = null;
        IBlockchainExplorerHelper explorerHelper = null;

        public FeeManager(ITransactionBroacaster _transactionBroadcaster, IBlockchainExplorerHelper _explorerHelper)
        {
            transactionBroadcaster = _transactionBroadcaster;
            explorerHelper = _explorerHelper;
        }
        public async Task<Error.Error> GenerateFees(BitcoinSecret sourceSecret, BitcoinSecret destinationSecret, int feeCount)
        {
            var coins = await explorerHelper.GetCoinsForWallet(sourceSecret.GetAddress().ToWif(), 10, 0, null, null,
                 null, true) as GetOrdinaryCoinsForWalletReturnType;

            if (coins.Error == null)
            {
                var selectedCoin = coins.Coins.Where(c => c.Amount >= (ulong)feeCount
                * Constants.BTCToSathoshiMultiplicationFactor).FirstOrDefault();
                if (selectedCoin == null)
                {
                    Error.Error retError = new Error.Error();
                    retError.Message = "Could not find the proper coin to spend.";
                    return retError;
                }

                TransactionBuilder builder = new TransactionBuilder();
                builder.AddKeys(sourceSecret).AddCoins(selectedCoin);
                for (int i = 0; i < feeCount; i++)
                {
                    builder.Send(destinationSecret.GetAddress(),
                        new Money(Constants.BTCToSathoshiMultiplicationFactor));
                }
                builder.SetChange(sourceSecret.GetAddress());
                builder.SendFees(new Money(feeCount * 100000));

                var tx = builder.BuildTransaction(true);
                await transactionBroadcaster.BroadcastTransactionToBlockchain(tx.ToHex());
                return null; // No error
            }

            return coins.Error;
        }

        public async Task<IExtendedCoin[]> GetFeeCoinsToAddToTransaction(Transaction tx)
        {
            return null;
        }
    }
}
