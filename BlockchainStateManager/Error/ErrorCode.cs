using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Error
{
    public enum ErrorCode
    {
        Exception,
        ProblemInRetrivingWalletOutput,
        ProblemInRetrivingTransaction,
        NotEnoughBitcoinAvailable,
        NotEnoughAssetAvailable,
        PossibleDoubleSpend,
        AssetNotFound,
        TransactionNotSignedProperly,
        BadInputParameter,
        PersistantConcurrencyProblem,
        NoCoinsToRefund,
        NoCoinsFound,
        InvalidAddress
    }
}
