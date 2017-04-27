using BlockchainStateManager.Models;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Helpers
{
    public interface IFeeManager
    {
        Task<Error.Error> GenerateFees(BitcoinSecret sourceSecret, BitcoinSecret destinationSecret, int feeCount);

        Task<IExtendedCoin[]> GetFeeCoinsToAddToTransaction(Transaction tx);
    }
}
