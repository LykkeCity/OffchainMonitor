using Core.Bitcoin;
using Core.QBitNinja;
using Core.Repositories.Settings;
using LkeServices.Transactions;
using LkeServices.Triggers.Attributes;
using NBitcoin;
using QBitNinja.Client.Models;
using SqlliteRepositories.Model;
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
        IRpcBitcoinClient rpcBitcoinClient;

        public CommitmentBroadcastCheck(ISettingsRepository _settingsRepository,
            IBitcoinTransactionService _bitcoinTransactionService, IQBitNinjaApiCaller _qBitNinjaApiCaller,
            IRpcBitcoinClient _rpcBitcoinClient)
        {
            settingsRepository = _settingsRepository;
            bitcoinTransactionService = _bitcoinTransactionService;
            qBitNinjaApiCaller = _qBitNinjaApiCaller;
            rpcBitcoinClient = _rpcBitcoinClient;
        }
        [TimerTrigger("00:00:10")]
        public async Task CheckCommitmentBroadcast()
        {
            string multisig = null;
            Network network = Network.TestNet;
            try
            {
                multisig = await settingsRepository.Get<string>("multisig");
                network = await Helper.GetNetwork(settingsRepository);
            }
            catch(Exception exp)
            {
                return;
            }

            var outputs = await qBitNinjaApiCaller.GetAddressBalance(multisig, true, true);

            MultisigOutputEntity desiredOutput = null;
            using (OffchainMonitorContext context = new OffchainMonitorContext())
            {
                desiredOutput = (from o in context.MultisigOutputs
                                 orderby o.LastSeen ascending
                                 select o).FirstOrDefault();
            }

            if (desiredOutput == null)
            {
                // There is no commitment
                return;
            }
            else
            {
                foreach (var op in outputs.Operations)
                {
                    var found = op.ReceivedCoins.Where(o => o.Outpoint.Hash.ToString() == desiredOutput.TransactionId && o.Outpoint.N == desiredOutput.OutputNumber).Any();
                    if (found)
                    {
                        // The output is still unspent which means no commitments is spent
                        return;
                    }
                }
            }

            // The output has not been found, which means it has been spent by a commitment
            outputs = await qBitNinjaApiCaller.GetAddressBalance(multisig, true, false);
            foreach (var op in outputs.Operations)
            {
                bool found = op.SpentCoins.
                    Where(sc => sc.Outpoint.Hash.ToString() == desiredOutput.TransactionId && sc.Outpoint.N == desiredOutput.OutputNumber).Any();

                if (found)
                {
                    var respectivePunishment = await FindThePunishment(op, desiredOutput);
                    if (respectivePunishment != null)
                    {
                        await rpcBitcoinClient.BroadcastTransaction(respectivePunishment, new Guid());
                    }
                    else
                    {
                        // The punishment has not been found
                    }
                }
            }
        }

        // ToDo: The finding of transaction in DB will probably has a better algorithm
        // The following one by one version is the preliminary design
        private async Task<Transaction> FindThePunishment(BalanceOperation op, MultisigOutputEntity output)
        {
            using (OffchainMonitorContext context = new OffchainMonitorContext())
            {
                var commitments = from c in context.Commitments
                                  where c.CommitmentOutput.TransactionId == output.TransactionId &&
                                  c.CommitmentOutput.OutputNumber == output.OutputNumber
                                  select c;

                if (commitments != null && commitments.Count() > 0)
                {
                    foreach (var c in commitments)
                    {
                        if (TransactionsMatch(c, op))
                        {
                            return new Transaction(c.Punishment);
                        }
                    }
                }
                else
                {
                    // No commitment cosuming the specified output
                    return null;
                }

                // No commitment found matching the specified transaction, spending multisig output
                return null;
            }
        }

        private bool TransactionsMatch(CommitmentEntity c, BalanceOperation op)
        {
            var commitmentTx = new Transaction(c.Commitment);

            foreach (var input in commitmentTx.Inputs)
            {
                if (!op.SpentCoins.Select(sp => sp.Outpoint.Hash == input.PrevOut.Hash && sp.Outpoint.N == input.PrevOut.N).Any())
                {
                    return false;
                }
            }

            foreach(var output in commitmentTx.Outputs)
            {
                if(!op.ReceivedCoins.Select(rc => rc.GetScriptCode() == output.ScriptPubKey).Any())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
