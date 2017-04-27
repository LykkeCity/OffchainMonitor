using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Models
{
    public class Multisig
    {
        public string MultiSigAddress
        {
            get;
            set;
        }

        public string MultiSigScript
        {
            get;
            set;
        }

        public string WalletAddress
        {
            get;
            set;
        }
    }

}
