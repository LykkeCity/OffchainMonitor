using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Repositories.Transactions
{
    public interface IBroadcastedTransaction
    {
        string Hash { get; }
        Guid TransactionId { get; }
    }

    public interface IBroadcastedTransactionRepository
    {
        Task InsertTransaction(string hash, Guid transactionId);

        Task<IBroadcastedTransaction> GetTransaction(string hash);

        Task SaveToBlob(Guid transactionId, string hex);

        Task<bool> IsBroadcasted(Guid transactionId);
    }

    public class DummyBroadcastedTransactionRepository : IBroadcastedTransactionRepository
    {
        public Task InsertTransaction(string hash, Guid transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<IBroadcastedTransaction> GetTransaction(string hash)
        {
            throw new NotImplementedException();
        }

        public Task SaveToBlob(Guid transactionId, string hex)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsBroadcasted(Guid transactionId)
        {
            throw new NotImplementedException();
        }
    }
}
