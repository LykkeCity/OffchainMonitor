using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockchainStateManager.Error;
using BlockchainStateManager.Assets;

namespace BlockchainStateManager.Models
{
    public class GetCoinsForWalletReturnType
    {
        public Error.Error Error
        {
            get;
            set;
        }

        public KeyStorage MatchingAddress
        {
            get;
            set;
        }

        public AssetDefinition Asset { get; set; }
    }
}
