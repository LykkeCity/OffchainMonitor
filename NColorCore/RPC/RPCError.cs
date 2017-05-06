using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NColorCore.RPC
{
    public class RPCError
    {
        public RPCErrorCode Code
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }
    }
}
