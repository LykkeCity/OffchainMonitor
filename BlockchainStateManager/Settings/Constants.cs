using NBitcoin;

namespace BlockchainStateManager.Settings
{
    public sealed class Constants
    {
        public const uint MinimumRequiredSatoshi = 50000; // 100000000 satoshi is one BTC
        public const uint TransactionSendFeesInSatoshi = 10000;
        public const ulong BTCToSathoshiMultiplicationFactor = 100000000;
        public const uint ConcurrencyRetryCount = 3;
        public const uint NBitcoinColoredCoinOutputInSatoshi = 2730;
        private const int LocktimeMinutesAllowance = 120;
        public static readonly BitcoinSecret USDAssetPrivateKey = new BitcoinSecret("cQc1KwWUg5jPZG8PC7xisJ82GSBdafpdhhNBvwSqZCcJuafX96BL"); // TestExchangeUSD;
        public const uint USDAssetMultiplicationFactor = 100;
    }
}
