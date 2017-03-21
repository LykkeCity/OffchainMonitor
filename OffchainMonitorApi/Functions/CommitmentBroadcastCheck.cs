using Core.Bitcoin;
using Core.QBitNinja;
using Core.Repositories.Settings;
using LkeServices.Transactions;
using LkeServices.Triggers.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OffchainMonitorApi.Functions
{
    public class CommitmentBroadcastCheck
    {
        ISettingsRepository settingsRepository;
        IBitcoinTransactionService bitcoinTransactionService;
        IQBitNinjaApiCaller qBitNinjaApiCaller;

        public CommitmentBroadcastCheck(ISettingsRepository _settingsRepository, 
            IBitcoinTransactionService _bitcoinTransactionService, IQBitNinjaApiCaller _qBitNinjaApiCaller)
        {
            settingsRepository = _settingsRepository;
            bitcoinTransactionService = _bitcoinTransactionService;
            qBitNinjaApiCaller = _qBitNinjaApiCaller;
        }
        [TimerTrigger("00:00:10")]
        public async Task CheckCommitmentBroadcast()
        {
            var multisig = await settingsRepository.Get<string>("multisig");
            var netwok = await Helper.GetNetwork(settingsRepository);

            var outputs = await qBitNinjaApiCaller.GetAddressBalance(multisig, true, true);
        }
    }
}
