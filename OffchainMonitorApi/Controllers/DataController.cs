using Core.Repositories.Settings;
using LkeServices.Transactions;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using SqlliteRepositories.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OffchainMonitorApi.Controllers
{
    [Route("api/[controller]")]
    public class DataController : Controller
    {
        IBitcoinTransactionService bitcoinTransactionService;
        ISettingsRepository settingsRepository;

        public DataController(ISettingsRepository _settingsRepository,
            IBitcoinTransactionService _bitcoinTransactionService) : base()
        {
            bitcoinTransactionService = _bitcoinTransactionService;
            settingsRepository = _settingsRepository;
        }

        private async Task<Network> GetNetwork()
        {
            string network = await settingsRepository.Get<string>("network");
            switch(network)
            {
                case "main":
                    return Network.Main;
                default:
                    return Network.TestNet;
            }
        }

        [HttpGet("AddCommitmentPunishmentPair")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AddCommitmentPunishmentPair([FromQuery]string commitment,
            [FromQuery]string punishment)
        {
            if (string.IsNullOrEmpty(commitment) || string.IsNullOrEmpty(punishment))
            {
                return BadRequest("Passed parameters should not be null or empty");
            }
            else
            {
                Transaction cTx = new Transaction(commitment);
                Transaction pTx = new Transaction(punishment);

                try
                {
                    using (OffchainMonitorContext context = new OffchainMonitorContext())
                    {
                        var newlyAddedCommitment = await context.Commitments.AddAsync(new CommitmentEntity
                        {
                            Commitment = commitment,
                            Punishment = punishment
                        });

                        bool found = false;
                        var multisig = await settingsRepository.Get<string>("multisig");
                        var netwok = await GetNetwork();
                        for (int i = 0; i < cTx.Inputs.Count; i++)
                        {
                            var prevHash = cTx.Inputs[i].PrevOut.Hash.ToString();
                            var prevTx = await bitcoinTransactionService.GetTransaction(prevHash);
                            var prevOut = cTx.Inputs[i].PrevOut;
                            if (prevTx.Outputs[prevOut.N].ScriptPubKey.GetDestinationAddress(netwok).ToWif()
                                == multisig)
                            {
                                found = true;

                                var existingMultisigOutput = (from o in context.MultisigOutputs
                                                              where o.TransactionId == prevOut.Hash.ToString() && o.OutputNumber == prevOut.N
                                                              select o).FirstOrDefault();
                                if(existingMultisigOutput == null)
                                {
                                    existingMultisigOutput = new MultisigOutputEntity();
                                    existingMultisigOutput.TransactionId = prevOut.Hash.ToString();
                                    existingMultisigOutput.OutputNumber = (int) prevOut.N;
                                    await context.MultisigOutputs.AddAsync(existingMultisigOutput); ;
                                }

                                CommitmentMultisigOutput commitmetMultisigOutput
                                    = new CommitmentMultisigOutput { CommitmentId = newlyAddedCommitment.Entity.Id,
                                        MultisigOutputTxId = prevOut.Hash.ToString(), Outputumber = (int) prevOut.N };

                                await context.CommitmentMultisigOutput.AddAsync(commitmetMultisigOutput);
                            }
                        }

                        if(!found)
                        {
                            return BadRequest(string.Format("The provide transaction does not pay to multisig:{0}", multisig));
                        }

                        await context.SaveChangesAsync();
                    }
                    return Ok();
                }
                catch (Exception exp)
                {
                    throw exp;
                }
            }
        }
    }
}
