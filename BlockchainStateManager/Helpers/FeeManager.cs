using BlockchainStateManager.Models;
using BlockchainStateManager.Settings;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockchainStateManager.Models;
using BlockchainStateManager.DB;

namespace BlockchainStateManager.Helpers
{
    public static class FeeManagerExtenions
    {
        
        public static async Task<TransactionBuilder> AddEnoughPaymentFee(this TransactionBuilder builder, string changeAddress)
        {
            var selectedFee = await FeeManager.GetOneFeeCoin();

            builder.SetChange(BitcoinAddress.Create(changeAddress), ChangeType.Uncolored);
            Coin selectedCoin = new Coin(new uint256(selectedFee.TransactionId), (uint) selectedFee.OutputNumber,
                    new Money(selectedFee.Satoshi), new Script(selectedFee.Script));
            builder.AddKeys(new BitcoinSecret(selectedFee.PrivateKey)).AddCoins(selectedCoin);
            return builder;
        }
    }
    public class FeeManager : IFeeManager
    {
        ITransactionBroacaster transactionBroadcaster = null;
        IBlockchainExplorerHelper explorerHelper = null;

        public static async Task<IList<Fee>> GetFeesForTransaction(Transaction tx)
        {
            var fees = new List<Fee>();
            fees.Add(await GetOneFeeCoin());

            return fees;
        }

        public static async Task<Fee> GetOneFeeCoin()
        {
            using (BlockchainStateManagerContext context = new BlockchainStateManagerContext())
            {
                var selectedFee = (from f in context.Fees
                                   where f.Consumed == false
                                   select f).FirstOrDefault();

                if(selectedFee == null)
                {
                    throw new Exception("No proper fee was found.");
                }

                selectedFee.Consumed = true;

                await context.SaveChangesAsync();

                return selectedFee;
            }
        }

        public FeeManager(ITransactionBroacaster _transactionBroadcaster, IBlockchainExplorerHelper _explorerHelper)
        {
            transactionBroadcaster = _transactionBroadcaster;
            explorerHelper = _explorerHelper;
        }
        public async Task<Error.Error> GenerateFees(BitcoinSecret sourceSecret, BitcoinSecret destinationSecret, int feeCount)
        {
            var feeAmount = Constants.BTCToSathoshiMultiplicationFactor / 100;
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

                try
                {
                    TransactionBuilder builder = new TransactionBuilder();
                    builder.AddKeys(sourceSecret).AddCoins(selectedCoin);
                    for (int i = 0; i < feeCount; i++)
                    {
                        builder.Send(destinationSecret.GetAddress(),
                            new Money(feeAmount));
                    }
                    builder.SetChange(sourceSecret.GetAddress());
                    builder.SendFees(new Money(feeCount * 100000));

                    var tx = builder.BuildTransaction(true);
                    await transactionBroadcaster.BroadcastTransactionToBlockchain(tx.ToHex());

                    using (BlockchainStateManagerContext context = new BlockchainStateManagerContext())
                    {
                        IList<Fee> fees = new List<Fee>();
                        var txHash = tx.GetHash().ToString();
                        for (uint i = 0; i < feeCount; i++)
                        {
                            fees.Add(new Fee
                            {
                                Consumed = false,
                                TransactionId = txHash,
                                OutputNumber = (int) i,
                                Satoshi = (long) feeAmount,
                                PrivateKey = destinationSecret.ToString(),
                                Script = tx.Outputs[i].ScriptPubKey.ToString()
                            });
                        }
                        context.Fees.AddRange(fees);

                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception exp)
                {
                    Error.Error retError = new Error.Error();
                    retError.Message = string.Format("An exception occured {0}.", exp.ToString());
                    return retError;
                }

                return null; // No error
            }

            return coins.Error;
        }

        public async Task<Fee[]> GetFeeCoinsToAddToTransaction(Transaction tx)
        {
            return new Fee[] { await GetOneFeeCoin() };
        }
    }
}
