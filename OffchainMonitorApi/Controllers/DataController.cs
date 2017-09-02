using Core.Repositories.Settings;
using LkeServices.Transactions;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using OffchainMonitorApi.Models;
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

        // CommitmentTxId = (new Transaction(commitment)).GetHash().ToString(),
        [HttpGet("AddCommitmentPunishmentPair")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AddCommitmentPunishmentPair([FromQuery]string commitment,
            [FromQuery]string punishment, [FromQuery]bool overwrite = false)
        {
            if (string.IsNullOrEmpty(commitment) || string.IsNullOrEmpty(punishment))
            {
                return BadRequest("Passed parameters should not be null or empty");
            }
            else
            {
                try
                {
                    Transaction cTx = new Transaction(commitment);
                    Transaction pTx = new Transaction(punishment);

                    using (OffchainMonitorContext context = new OffchainMonitorContext())
                    {
                        var matchedCommitment = (from c in context.Commitments
                                                 where c.CommitmentTxId == cTx.GetHash().ToString()
                                                 select c).FirstOrDefault();

                        if (matchedCommitment != null)
                        {
                            if (overwrite == false)
                            {
                                return BadRequest("To overwrite an existing punishment for existing commitment id, the overwrite flag should bet set to true");
                            }
                            else
                            {
                                matchedCommitment.Commitment = commitment;
                                matchedCommitment.Punishment = punishment;
                            }
                        }
                        else
                        {
                            var newlyAddedCommitment = new CommitmentEntity
                            {
                                Commitment = commitment,
                                CommitmentTxId = cTx.GetHash().ToString(),
                                Punishment = punishment
                            };

                            await context.Commitments.AddAsync(newlyAddedCommitment);
                        }

                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception exp)
                {
                    throw exp;
                }

                return Ok();
            }
        }

        // Until SegWit acivation since punishment requires commitment id, and it is not available until hub broadcasts it, this is not usable
        // After segwit activation commitment id will be available independent of its signing
        [HttpGet("AddCommitmentPunishmentPairOldDesign")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AddCommitmentPunishmentPairOldDesign([FromQuery]string commitment,
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
                            if (prevTx.Outputs[prevOut.N].ScriptPubKey.GetDestinationAddress(netwok).ToString()
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

        [HttpPost("AddFee")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AddFee([FromBody]AddFeeRequest request)
        {
            var checkResult = request.CheckModel();
            if (checkResult != null)
            {
                return StatusCode(500, checkResult);
            }
            return Ok();
        }
    }
}
