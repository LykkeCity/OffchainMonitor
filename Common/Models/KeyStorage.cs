using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public partial class KeyStorage
    {
        public string WalletAddress { get; set; }
        public string WalletPrivateKey { get; set; }
        public string MultiSigAddress { get; set; }
        public string MultiSigScript { get; set; }
        public string ExchangePrivateKey { get; set; }
        public string Network { get; set; }
        public byte[] Version { get; set; }
    }
}
