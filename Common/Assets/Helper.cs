using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Assets
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

        public static AssetDefinition GetAssetFromName(AssetDefinition[] assets, string assetName)
        {
            AssetDefinition ret = null;

            foreach (var item in assets)
            {
                if (item.Name == assetName)
                {
                    ret = new AssetDefinition();
                    ret.AssetId = item.AssetId;
                    ret.PrivateKey = item.PrivateKey;
                    ret.AssetAddress = item.AssetAddress;
                    ret.Divisibility = item.Divisibility;
                    ret.DefinitionUrl = item.DefinitionUrl;
                    break;
                }
            }

            return ret;
        }
    }
}
