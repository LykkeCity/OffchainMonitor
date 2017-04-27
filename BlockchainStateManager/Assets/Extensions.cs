using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using static BlockchainStateManager.Helper;

namespace BlockchainStateManager.Assets
{
    public static class Extentions
    {
        public static AssetDefinition GetAssetFromName(this AssetDefinition[] assets, string assetName, Network network)
        {
            AssetDefinition ret = null;
            foreach (var item in assets)
            {
                if (item.Name == assetName)
                {
                    ret = new AssetDefinition();
                    ret.AssetId = item.AssetId;
                    ret.PrivateKey = item.PrivateKey;
                    ret.AssetAddress = (new BitcoinSecret(ret.PrivateKey, network)).PubKey.
                        GetAddress(network).ToString();
                    ret.Divisibility = item.Divisibility;
                    ret.DefinitionUrl = item.DefinitionUrl;
                    break;
                }
            }

            return ret;
        }
    }
}
