using Common.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class GetCoinsForWalletReturnType
    {
        public Error.Error Error
        {
            get;
            set;
        }

        public KeyStorage MatchingAddress
        {
            get;
            set;
        }

        public AssetDefinition Asset { get; set; }
    }
}
