using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class GetScriptCoinsForWalletReturnType : GetCoinsForWalletReturnType
    {
        public ColoredCoin[] AssetScriptCoins
        {
            get;
            set;
        }

        public ScriptCoin[] ScriptCoins
        {
            get;
            set;
        }
    }
}
