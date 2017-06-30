using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class QBitNinjaOutputResponse
    {
        public object continuation { get; set; }
        public List<QBitNinjaOperation> operations { get; set; }
    }

    public class QBitNinjaOperation
    {
        public long amount { get; set; }
        public int confirmations { get; set; }
        public int height { get; set; }
        public string blockId { get; set; }
        public string transactionId { get; set; }
        public List<QBitNinjaReceivedCoin> receivedCoins { get; set; }
        public List<QBitNinjaSpentCoin> spentCoins { get; set; }
    }

    public class QBitNinjaReceivedCoin
    {
        public string transactionId { get; set; }
        public int index { get; set; }
        public long value { get; set; }
        public string scriptPubKey { get; set; }
        public object redeemScript { get; set; }
        public string assetId { get; set; }
        public long quantity { get; set; }
    }

    public class QBitNinjaSpentCoin
    {
        public string address { get; set; }
        public string transactionId { get; set; }
        public int index { get; set; }
        public long value { get; set; }
        public string scriptPubKey { get; set; }
        public object redeemScript { get; set; }
        public string assetId { get; set; }
        public long quantity { get; set; }
    }

    public class QBitNinjaUnspentOutput : UniversalUnspentOutput
    {
        public string transaction_hash { get; set; }
        public int output_index { get; set; }
        public long value { get; set; }
        public int confirmations { get; set; }
        public string script_hex { get; set; }
        public string asset_id { get; set; }
        public long asset_quantity { get; set; }
    }
}
