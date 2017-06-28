using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OffchainMonitorApi.Models
{
    public class AddFeeRequest
    {
        public string TransactionId
        {
            get;
            set;
        }

        public int OutputNumber
        {
            get;
            set;
        }

        public string PrivateKey
        {
            get;
            set;
        }

        public string CheckModel()
        {
            if(string.IsNullOrEmpty(TransactionId))
            {
                return "Transaction id for the fee should not be null or empty.";
            }

            if(string.IsNullOrEmpty(PrivateKey))
            {
                return "Private key should not be null or empty.";
            }

            return null;
        }
    }
}
