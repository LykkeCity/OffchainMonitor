using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Models
{
    public class GetOrdinaryCoinsForWalletReturnType : GetCoinsForWalletReturnType
    {
        public ColoredCoin[] AssetCoins
        {
            get;
            set;
        }

        public Coin[] Coins
        {
            get;
            set;
        }
    }
}
