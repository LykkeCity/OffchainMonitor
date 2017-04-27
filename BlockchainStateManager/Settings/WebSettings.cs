using BlockchainStateManager.Models;

namespace BlockchainStateManager.Settings
{
    public static class WebSettings
    {
        public static RPCConnectionParams ConnectionParams
        {
            get;
            set;
        }

        /*
        public static Core.AssetDefinition[] Assets
        {
            get;
            set;
        }
        */

        public static string FeeAddress
        {
            get;
            set;
        }

        public static string ConnectionString
        {
            get;
            set;
        }

        public static int SwapMinimumConfirmationNumber
        {
            get;
            set;
        }

        public static bool UseSegKeysTable
        {
            get;
            set;
        }
    }
}
