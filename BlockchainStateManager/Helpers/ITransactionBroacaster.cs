using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Helpers
{
    public interface ITransactionBroacaster
    {
       Task BroadcastTransactionToBlockchain(string transaction);
    }
}
