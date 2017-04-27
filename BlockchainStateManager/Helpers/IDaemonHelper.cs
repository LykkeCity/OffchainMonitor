using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Helpers
{
    public interface IDaemonHelper
    {
        Task<IEnumerable<string>> GenerateBlocks(int count);
        Task<Tuple<bool, string, string>> GetTransactionHex(string transactionId);
    }
}
