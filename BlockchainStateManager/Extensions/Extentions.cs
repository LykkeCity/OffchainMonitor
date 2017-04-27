using Autofac;
using BlockchainStateManager.Enum;
using BlockchainStateManager.Models;
using BlockchainStateManager.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Extensions
{
    public static class Extentions
    {
        public static int GetOutputIndex(this UniversalUnspentOutput output)
        {
            switch (Settings.Settings.ApiProvider)
            {
                case APIProvider.QBitNinja:
                    return ((QBitNinjaUnspentOutput)output).output_index;
                default:
                    throw new Exception("Not supported.");
            }
        }

        public static string GetScriptHex(this UniversalUnspentOutput output)
        {
            switch (Settings.Settings.ApiProvider)
            {
                case APIProvider.QBitNinja:
                    return ((QBitNinjaUnspentOutput)output).script_hex;
                default:
                    throw new Exception("Not supported.");
            }
        }

        internal static string GetAssetId(this UniversalUnspentOutput item)
        {
            switch (Settings.Settings.ApiProvider)
            {
                case APIProvider.QBitNinja:
                    return ((QBitNinjaUnspentOutput)item).asset_id;
                default:
                    throw new Exception("Not supported.");
            }
        }

        internal static long GetValue(this UniversalUnspentOutput output)
        {
            switch (Settings.Settings.ApiProvider)
            {
                case APIProvider.QBitNinja:
                    return ((QBitNinjaUnspentOutput)output).value;
                default:
                    throw new Exception("Not supported.");
            }
        }

        internal static string GetTransactionHash(this UniversalUnspentOutput output)
        {
            switch (Settings.Settings.ApiProvider)
            {
                case APIProvider.QBitNinja:
                    return ((QBitNinjaUnspentOutput)output).transaction_hash;
                default:
                    throw new Exception("Not supported.");
            }
        }

        internal static int GetConfirmationNumber(this UniversalUnspentOutput item)
        {
            switch (Settings.Settings.ApiProvider)
            {
                case APIProvider.QBitNinja:
                    return ((QBitNinjaUnspentOutput)item).confirmations;
                default:
                    throw new Exception("Not supported.");
            }
        }

        internal static long GetAssetAmount(this UniversalUnspentOutput item)
        {
            switch (Settings.Settings.ApiProvider)
            {
                case APIProvider.QBitNinja:
                    return ((QBitNinjaUnspentOutput)item).asset_quantity;
                default:
                    throw new Exception("Not supported.");
            }
        }

        internal static long GetBitcoinAmount(this UniversalUnspentOutput item)
        {
            switch (Settings.Settings.ApiProvider)
            {
                case APIProvider.QBitNinja:
                    return ((QBitNinjaUnspentOutput)item).value;
                default:
                    throw new Exception("Not supported.");
            }
        }
    }
}
