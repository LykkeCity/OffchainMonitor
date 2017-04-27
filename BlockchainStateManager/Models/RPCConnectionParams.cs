using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Models
{
    public class RPCConnectionParams
    {
        public string Username
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public string IpAddress
        {
            get;
            set;
        }

        public string Network
        {
            get;
            set;
        }

        public Network BitcoinNetwork
        {
            get
            {
                switch (Network.ToLower())
                {
                    case "main":
                        return NBitcoin.Network.Main;
                    case "testnet":
                        return NBitcoin.Network.TestNet;
                    default:
                        throw new NotImplementedException(string.Format("Bitcoin network {0} is not supported.", Network));
                }
            }
        }
    }

}
