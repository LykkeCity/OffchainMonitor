using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Assets
{
    public class AssetDefinition
    {
        public string AssetId { get; set; }
        public string AssetAddress { get; set; }
        public string Name { get; set; }
        public string PrivateKey { get; set; }
        public string DefinitionUrl { get; set; }
        public int? Divisibility { get; set; }
        [JsonIgnore]
        public long MultiplyFactor
        {
            get
            {
                return (long)Math.Pow(10, Divisibility ?? 0);
            }
        }
    }

    // The returned object is a Tuple with first parameter specifing if an error has occured,
    // second the error message and third the transaction hex
    /* 

     public class Asset
     {
         public string AssetId
         {
             get;
             set;
         }

         public BitcoinAddress AssetAddress
         {
             get;
             set;
         }

         public long AssetMultiplicationFactor
         {
             get;
             set;
         }

         public string AssetDefinitionUrl { get; set; }

         public string AssetPrivateKey { get; set; }
     }
     */
}
