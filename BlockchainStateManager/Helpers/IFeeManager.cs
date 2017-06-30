using BlockchainStateManager.DB;
using BlockchainStateManager.Models;
using Common.Error;
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
        Task<Error> GenerateFees(BitcoinSecret sourceSecret, BitcoinSecret destinationSecret, int feeCount);

        Task<Fee[]> GetFeeCoinsToAddToTransaction(Transaction tx);
    }
}
