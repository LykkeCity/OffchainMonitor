using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NColorCore.RPC
{
    public class RPCResponse
    {
        public RPCResponse(string result, RPCError error)
        {
            Result = result;
            Error = error;
        }
        public RPCError Error
        {
            get;
            set;
        }

        public string Result
        {
            get;
            set;
        }

        public string ResultString
        {
            get
            {
                if (Result == null)
                    return null;
                return Result.ToString();
            }
        }

        public void ThrowIfError()
        {
            if (Error != null)
            {
                throw new RPCException(Error.Code, Error.Message, this);
            }
        }
    }
}
