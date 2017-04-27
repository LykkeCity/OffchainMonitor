using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Models
{
    public interface IExtendedCoin : ICoin
    {
        BitcoinSecret Secret { get; set; }
    }
}
