using BlockchainStateManager.Models;
using BlockchainStateManager.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Helpers
{
    public class QBitNinjaHelper : AbstractBlockchainExplorerHelper
    {
        public QBitNinjaHelper(ISettingsProvider _settingsProvider) :
            base(_settingsProvider)
        {
        }
        public async Task<bool> IsUrlSuccessful(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage result = await client.GetAsync(url);
                if (!result.IsSuccessStatusCode)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public override async Task<bool> HasTransactionIndexed(string txId, string dummy)
        {
            var settings = SettingsProvider.GetSettings();

            string url = settings.QBitNinjaBaseUrl + "transactions/" + txId + "?colored=true";
            return await IsUrlSuccessful(url);
        }

        public override async Task<bool> HasBlockIndexed(string blockId, string dummy)
        {
            var settings = SettingsProvider.GetSettings();

            string url = settings.QBitNinjaBaseUrl + "blocks/" + blockId + "?headeronly=true";
            return await IsUrlSuccessful(url);
        }

        public override async Task<bool> HasBalanceIndexed(string txId, string btcAddress)
        {
            var settings = SettingsProvider.GetSettings();

            return await HasBalanceIndexedInternal(txId, btcAddress);
        }

        public override async Task<bool> HasBalanceIndexedZeroConfirmation(string txId, string btcAddress)
        {
            var settings = SettingsProvider.GetSettings();

            return await HasBalanceIndexedInternal(txId, btcAddress, false);
        }

        public async Task<bool> HasBalanceIndexedInternal(string txId, string btcAddress,
            bool confirmationRequired = true)
        {
            HttpResponseMessage result = null;
            bool exists = false;

            var settings = SettingsProvider.GetSettings();

            using (HttpClient client = new HttpClient())
            {
                string url = null;
                exists = false;
                url = settings.QBitNinjaBaseUrl + "balances/" + btcAddress + "?unspentonly=true&colored=true";
                result = await client.GetAsync(url);
            }

            if (!result.IsSuccessStatusCode)
            {
                return false;
            }
            else
            {
                var webResponse = await result.Content.ReadAsStringAsync();
                var notProcessedUnspentOutputs = Newtonsoft.Json.JsonConvert.DeserializeObject<QBitNinjaOutputResponse>
                    (webResponse);
                if (notProcessedUnspentOutputs.operations != null && notProcessedUnspentOutputs.operations.Count > 0)
                {
                    notProcessedUnspentOutputs.operations.ForEach((o) =>
                    {
                        exists = o.receivedCoins
                       .Where(c => c.transactionId.Equals(txId) && (!confirmationRequired | o.confirmations > 0))
                       .Any() | exists;
                        if (exists)
                        {
                            return;
                        }
                    });
                }

                return exists;
            }
        }

        public override async Task WaitUntillBlockchainExplorerHasIndexed(Func<string, string, Task<bool>> checkIndexed,
            IEnumerable<string> ids, string id2 = null)
        {
            var indexed = false;

            var settings = SettingsProvider.GetSettings();

            foreach (var id in ids)
            {
                indexed = false;
                for (int i = 0; i < 30; i++)
                {
                    bool result = false;
                    try
                    {
                        result = await checkIndexed(id, id2);
                    }
                    catch (Exception exp)
                    {

                    }

                    if (result)
                    {
                        indexed = true;
                        break;
                    }
                    await Task.Delay(1000);
                }

                if (!indexed)
                {
                    throw new Exception(string.IsNullOrEmpty(id2) ? string.Format("Item with id: {0} did not get indexed yet.", id) : string.Format("Item with id: {0} did not get indexed yet. Provided id2 is {1}", id, id2));
                }
            }
        }

        public override async Task<Tuple<UniversalUnspentOutput[], bool, string>> GetWalletOutputs(string walletAddress)
        {
            Tuple<UniversalUnspentOutput[], bool, string> ret = null;

            var qbitResult = await GetWalletOutputsQBitNinja(walletAddress);
            ret = new Tuple<UniversalUnspentOutput[], bool, string>(qbitResult.Item1 != null ? qbitResult.Item1.Select(c => (UniversalUnspentOutput)c).ToArray() : null,
                qbitResult.Item2, qbitResult.Item3);
            return ret;
        }

        public async Task<Tuple<QBitNinjaUnspentOutput[], bool, string>> GetWalletOutputsQBitNinja(string walletAddress)
        {
            bool errorOccured = false;
            string errorMessage = string.Empty;

            var settings = SettingsProvider.GetSettings();

            IList<QBitNinjaUnspentOutput> unspentOutputsList = new List<QBitNinjaUnspentOutput>();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = null;
                    url = settings.QBitNinjaBalanceUrl + walletAddress;
                    HttpResponseMessage result = await client.GetAsync(url + "?unspentonly=true&colored=true");
                    if (!result.IsSuccessStatusCode)
                    {
                        errorOccured = true;
                        errorMessage = result.ReasonPhrase;
                    }
                    else
                    {
                        var webResponse = await result.Content.ReadAsStringAsync();
                        var notProcessedUnspentOutputs = Newtonsoft.Json.JsonConvert.DeserializeObject<QBitNinjaOutputResponse>
                            (webResponse);
                        if (notProcessedUnspentOutputs.operations != null && notProcessedUnspentOutputs.operations.Count > 0)
                        {
                            notProcessedUnspentOutputs.operations.ForEach((o) =>
                            {
                                var convertResult = o.receivedCoins.Select(c => new QBitNinjaUnspentOutput
                                {
                                    confirmations = o.confirmations,
                                    output_index = c.index,
                                    transaction_hash = c.transactionId,
                                    value = c.value,
                                    script_hex = c.scriptPubKey,
                                    asset_id = c.assetId,
                                    asset_quantity = c.quantity
                                });
                                ((List<QBitNinjaUnspentOutput>)unspentOutputsList).AddRange(convertResult);
                            });
                        }
                        else
                        {
                            errorOccured = true;
                            errorMessage = "No coins to retrieve.";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                errorOccured = true;
                errorMessage = e.ToString();
            }

            return new Tuple<QBitNinjaUnspentOutput[], bool, string>(unspentOutputsList.ToArray(), errorOccured, errorMessage);
        }

        public override async Task<Tuple<float, float, bool, string>> GetAccountBalance(string walletAddress,
            string assetId)
        {
            float balance = 0;
            float unconfirmedBalance = 0;
            bool errorOccured = false;
            string errorMessage = "";
            string url;

            var settings = SettingsProvider.GetSettings();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    url = settings.QBitNinjaBalanceUrl + walletAddress;
                    HttpResponseMessage result = await client.GetAsync(url + "?unspentonly=true&colored=true");
                    if (!result.IsSuccessStatusCode)
                    {
                        return new Tuple<float, float, bool, string>(0, 0, true, result.ReasonPhrase);
                    }
                    else
                    {
                        var webResponse = await result.Content.ReadAsStringAsync();
                        QBitNinjaOutputResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<QBitNinjaOutputResponse>
                            (webResponse);
                        if (response.operations != null && response.operations.Count > 0)
                        {
                            foreach (var item in response.operations)
                            {
                                response.operations.ForEach((o) =>
                                {
                                    balance += o.receivedCoins.Where(c => !string.IsNullOrEmpty(c.assetId) && c.assetId.Equals(assetId) && o.confirmations > 0).Select(c => c.quantity).Sum();
                                    unconfirmedBalance += o.receivedCoins.Where(c => !string.IsNullOrEmpty(c.assetId) && c.assetId.Equals(assetId) && o.confirmations == 0).Select(c => c.quantity).Sum();
                                });
                            }
                        }
                        else
                        {
                            errorOccured = true;
                            errorMessage = "No coins found.";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                errorOccured = true;
                errorMessage = e.ToString();
            }
            return new Tuple<float, float, bool, string>(balance, unconfirmedBalance, errorOccured, errorMessage);
        }
    }
}
