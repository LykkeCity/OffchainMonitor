using Common.Enum;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Settings
{
    public interface ISettings
    {
        string QBitNinjaBaseUrl { get; set; }
        string QBitNinjaBalanceUrl { get; }
        string QBitNinjaTransactionUrl { get; }

        Network Network { get; set; }
        
    }
}
