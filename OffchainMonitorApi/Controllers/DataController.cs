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
                        var newlyAddedCommitment = new CommitmentEntity
                        {
                            Commitment = commitment,
                            Punishment = punishment
                        };

                        bool found = false;
                        var multisig = await settingsRepository.Get<string>("multisig");
                        var netwok = await Helper.GetNetwork(settingsRepository);
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
                                    var newMultisigOutput = new MultisigOutputEntity();
                                    newMultisigOutput.TransactionId = prevOut.Hash.ToString();
                                    newMultisigOutput.OutputNumber = (int) prevOut.N;
                                    newMultisigOutput.LastSeen = DateTime.UtcNow;
                                    await context.MultisigOutputs.AddAsync(newMultisigOutput);

                                    existingMultisigOutput = newMultisigOutput;
                                }

                                existingMultisigOutput.LastSeen = DateTime.UtcNow;
                                existingMultisigOutput.Commitments.Add(newlyAddedCommitment);

                                newlyAddedCommitment.CommitmentOutput = existingMultisigOutput;
                                await context.Commitments.AddAsync(newlyAddedCommitment);
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
