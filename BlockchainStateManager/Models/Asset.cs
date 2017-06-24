using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Models
{
    public class Asset
    {
        public string AssetId
        {
            get;
            set;
        }

        public uint MultiplicationFactor
        {
            get;
            set;
        }
    }
}
