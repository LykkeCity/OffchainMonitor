using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Assets
{
    public class Helper
    {
        public static bool IsRealAsset(string asset)
        {
            if (asset != null && asset.Trim().ToUpper() != "BTC")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
